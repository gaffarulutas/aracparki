using System.Data;
using AracParki.Application.Abstractions;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace AracParki.Infrastructure.Persistence;

public sealed class NpgsqlConnectionFactory(IConfiguration configuration) : IDbConnectionFactory
{
    private readonly string _connectionString = configuration.GetConnectionString("PostgreSQL")
                                                ?? throw new InvalidOperationException("Connection string 'PostgreSQL' is missing.");

    public async Task<IDbConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken)
    {
        var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }
}
