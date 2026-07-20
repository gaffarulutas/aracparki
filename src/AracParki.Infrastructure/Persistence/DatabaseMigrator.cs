using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace AracParki.Infrastructure.Persistence;

public sealed partial class DatabaseMigrator(
    IConfiguration configuration,
    IHostEnvironment environment,
    ILogger<DatabaseMigrator> logger)
{
    private static readonly Regex CopyFromStdin = CopyFromStdinRegex();

    public async Task MigrateAsync(CancellationToken cancellationToken = default)
    {
        if (!configuration.GetValue("Database:MigrateOnStartup", true))
        {
            logger.LogInformation("Database migrations skipped (Database:MigrateOnStartup=false)");
            return;
        }

        var scriptsDir = ResolveScriptsDirectory();
        var connectionString = configuration.GetConnectionString("PostgreSQL")
            ?? throw new InvalidOperationException("Connection string 'PostgreSQL' is missing.");

        await using var connection = new NpgsqlConnection(connectionString);
        await OpenWithRetryAsync(connection, cancellationToken);
        await EnsureMigrationsTableAsync(connection, cancellationToken);

        var applied = await LoadAppliedAsync(connection, cancellationToken);
        var scripts = Directory.GetFiles(scriptsDir, "*.sql")
            .OrderBy(static path => Path.GetFileName(path), StringComparer.Ordinal)
            .ToArray();

        if (scripts.Length == 0)
        {
            logger.LogWarning("No SQL scripts found in {ScriptsDir}", scriptsDir);
            return;
        }

        logger.LogInformation("Running database migrations from {ScriptsDir} ({Count} scripts)", scriptsDir, scripts.Length);

        foreach (var scriptPath in scripts)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var name = Path.GetFileName(scriptPath);
            var checksum = await ComputeChecksumAsync(scriptPath, cancellationToken);

            if (applied.TryGetValue(name, out var existingChecksum) &&
                string.Equals(existingChecksum, checksum, StringComparison.Ordinal))
            {
                continue;
            }

            if (!applied.ContainsKey(name) &&
                await CanSkipInitialApplyAsync(connection, name, cancellationToken))
            {
                await RecordAsync(connection, name, checksum, cancellationToken);
                logger.LogInformation("Recorded {Name} as applied (already present in database)", name);
                continue;
            }

            logger.LogInformation("Applying {Name}...", name);
            var sql = await File.ReadAllTextAsync(scriptPath, cancellationToken);
            await ExecuteScriptAsync(connection, sql, cancellationToken);
            await RecordAsync(connection, name, checksum, cancellationToken);
            logger.LogInformation("Applied {Name}", name);
        }
    }

    private string ResolveScriptsDirectory()
    {
        var configured = configuration["Database:ScriptsPath"];
        if (!string.IsNullOrWhiteSpace(configured))
        {
            var path = Path.IsPathRooted(configured)
                ? configured
                : Path.GetFullPath(Path.Combine(environment.ContentRootPath, configured));
            if (Directory.Exists(path))
            {
                return path;
            }

            throw new DirectoryNotFoundException($"Configured Database:ScriptsPath not found: {path}");
        }

        foreach (var candidate in EnumerateCandidateDirectories())
        {
            if (File.Exists(Path.Combine(candidate, "01_schema.sql")))
            {
                return candidate;
            }
        }

        throw new DirectoryNotFoundException(
            "Could not locate database/ SQL scripts. Set Database:ScriptsPath or copy scripts next to the app.");
    }

    private IEnumerable<string> EnumerateCandidateDirectories()
    {
        // Prefer the repo database/ folder during local development so SQL edits
        // apply on restart without requiring a rebuild/copy.
        var dir = new DirectoryInfo(environment.ContentRootPath);
        for (var i = 0; i < 6 && dir is not null; i++, dir = dir.Parent)
        {
            yield return Path.Combine(dir.FullName, "database");
        }

        yield return Path.Combine(environment.ContentRootPath, "database");
        yield return Path.Combine(AppContext.BaseDirectory, "database");
    }

    private async Task OpenWithRetryAsync(NpgsqlConnection connection, CancellationToken cancellationToken)
    {
        const int maxAttempts = 30;
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                await connection.OpenAsync(cancellationToken);
                return;
            }
            catch (Exception ex) when (attempt < maxAttempts && IsTransient(ex))
            {
                logger.LogWarning(
                    "Waiting for PostgreSQL (attempt {Attempt}/{Max}): {Message}",
                    attempt,
                    maxAttempts,
                    ex.Message);
                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
            }
        }
    }

    private static bool IsTransient(Exception ex) =>
        ex is NpgsqlException or TimeoutException or IOException
        || ex.InnerException is NpgsqlException or TimeoutException or IOException;

    private static async Task EnsureMigrationsTableAsync(NpgsqlConnection connection, CancellationToken cancellationToken)
    {
        const string sql = """
            CREATE TABLE IF NOT EXISTS schema_migrations (
                script_name  TEXT PRIMARY KEY,
                checksum     TEXT NOT NULL,
                applied_at   TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );
            """;

        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<Dictionary<string, string>> LoadAppliedAsync(
        NpgsqlConnection connection,
        CancellationToken cancellationToken)
    {
        const string sql = "SELECT script_name, checksum FROM schema_migrations;";
        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        while (await reader.ReadAsync(cancellationToken))
        {
            result[reader.GetString(0)] = reader.GetString(1);
        }

        return result;
    }

    private static async Task RecordAsync(
        NpgsqlConnection connection,
        string scriptName,
        string checksum,
        CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO schema_migrations (script_name, checksum, applied_at)
            VALUES (@name, @checksum, NOW())
            ON CONFLICT (script_name) DO UPDATE
            SET checksum = EXCLUDED.checksum,
                applied_at = EXCLUDED.applied_at;
            """;

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("name", scriptName);
        command.Parameters.AddWithValue("checksum", checksum);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<string> ComputeChecksumAsync(string path, CancellationToken cancellationToken)
    {
        await using var stream = File.OpenRead(path);
        var hash = await SHA256.HashDataAsync(stream, cancellationToken);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    /// <summary>
    /// Large one-shot seeds (neighborhoods) that were loaded via docker initdb should not be
    /// re-copied on first migrator boot; only re-run when the script checksum later changes.
    /// </summary>
    private static async Task<bool> CanSkipInitialApplyAsync(
        NpgsqlConnection connection,
        string scriptName,
        CancellationToken cancellationToken)
    {
        if (!string.Equals(scriptName, "04_neighborhoods.sql", StringComparison.Ordinal))
        {
            return false;
        }

        if (!await TableExistsAsync(connection, "neighborhoods", cancellationToken))
        {
            return false;
        }

        await using var command = new NpgsqlCommand("SELECT EXISTS (SELECT 1 FROM neighborhoods LIMIT 1);", connection);
        var exists = await command.ExecuteScalarAsync(cancellationToken);
        return exists is true;
    }

    private static async Task<bool> TableExistsAsync(
        NpgsqlConnection connection,
        string tableName,
        CancellationToken cancellationToken)
    {
        await using var command = new NpgsqlCommand(
            "SELECT to_regclass(@name) IS NOT NULL;",
            connection);
        command.Parameters.AddWithValue("name", $"public.{tableName}");
        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is true;
    }

    private static async Task ExecuteScriptAsync(
        NpgsqlConnection connection,
        string script,
        CancellationToken cancellationToken)
    {
        var matches = CopyFromStdin.Matches(script);
        if (matches.Count == 0)
        {
            await ExecuteSqlAsync(connection, script, cancellationToken);
            return;
        }

        var offset = 0;
        foreach (Match match in matches)
        {
            var before = script[offset..match.Index];
            if (!string.IsNullOrWhiteSpace(before))
            {
                await ExecuteSqlAsync(connection, before, cancellationToken);
            }

            var dataStart = match.Index + match.Length;
            var terminator = script.IndexOf("\n\\.", dataStart, StringComparison.Ordinal);
            if (terminator < 0)
            {
                throw new InvalidOperationException("COPY ... FROM STDIN block is missing a \\. terminator.");
            }

            var csvPayload = script[dataStart..terminator];
            var copyCommand = $"COPY {match.Groups[1].Value} ({match.Groups[2].Value}) FROM STDIN WITH ({match.Groups[3].Value})";
            await using (var writer = await connection.BeginTextImportAsync(copyCommand, cancellationToken))
            {
                await writer.WriteAsync(csvPayload.AsMemory(), cancellationToken);
            }

            offset = terminator + 3; // \n\.
            if (offset < script.Length && script[offset] == '\r')
            {
                offset++;
            }

            if (offset < script.Length && script[offset] == '\n')
            {
                offset++;
            }
        }

        var after = script[offset..];
        if (!string.IsNullOrWhiteSpace(after))
        {
            await ExecuteSqlAsync(connection, after, cancellationToken);
        }
    }

    private static async Task ExecuteSqlAsync(
        NpgsqlConnection connection,
        string sql,
        CancellationToken cancellationToken)
    {
        await using var command = new NpgsqlCommand(sql, connection)
        {
            CommandTimeout = 600
        };
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    [GeneratedRegex(
        @"COPY\s+([a-zA-Z_][\w.]*)\s*\(([^)]*)\)\s+FROM\s+STDIN\s+WITH\s*\(([^)]*)\)\s*;\s*\r?\n",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex CopyFromStdinRegex();
}
