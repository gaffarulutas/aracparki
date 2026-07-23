using AracParki.Application.Abstractions;
using AracParki.Application.Listings;
using AracParki.Application.Listings.Dtos;
using AracParki.Domain.Listings;
using Dapper;
using Npgsql;

namespace AracParki.Infrastructure.Listings;

public sealed class ListingReportRepository(IDbConnectionFactory connectionFactory) : IListingReportStore
{
    public async Task<long> CreateAsync(
        long listingId,
        string adNo,
        long reporterAccountId,
        string reasonCode,
        string? message,
        CancellationToken cancellationToken)
    {
        await using var connection = (System.Data.Common.DbConnection)
            await connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        try
        {
            return await connection.ExecuteScalarAsync<long>(
                new CommandDefinition(
                    """
                    INSERT INTO listing_reports (
                        listing_id, ad_no, reporter_account_id, reason_code, message, status
                    )
                    VALUES (
                        @ListingId, @AdNo, @ReporterAccountId, @ReasonCode, @Message, @Status
                    )
                    RETURNING id
                    """,
                    new
                    {
                        ListingId = listingId,
                        AdNo = adNo,
                        ReporterAccountId = reporterAccountId,
                        ReasonCode = reasonCode,
                        Message = message,
                        Status = ListingReportStatus.Open
                    },
                    cancellationToken: cancellationToken));
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw new InvalidOperationException("Bu ilan için zaten açık bir şikayetiniz var.");
        }
    }

    public async Task<bool> HasActiveReportAsync(
        long reporterAccountId,
        long listingId,
        CancellationToken cancellationToken)
    {
        await using var connection = (System.Data.Common.DbConnection)
            await connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        return await connection.ExecuteScalarAsync<bool>(
            new CommandDefinition(
                """
                SELECT EXISTS(
                    SELECT 1
                    FROM listing_reports
                    WHERE reporter_account_id = @ReporterAccountId
                      AND listing_id = @ListingId
                      AND status IN (@Open, @Reviewing)
                )
                """,
                new
                {
                    ReporterAccountId = reporterAccountId,
                    ListingId = listingId,
                    Open = ListingReportStatus.Open,
                    Reviewing = ListingReportStatus.Reviewing
                },
                cancellationToken: cancellationToken));
    }

    public async Task<ListingReportDetailDto?> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        await using var connection = (System.Data.Common.DbConnection)
            await connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        return await connection.QuerySingleOrDefaultAsync<ListingReportDetailDto>(
            new CommandDefinition(
                """
                SELECT
                    r.id AS Id,
                    r.listing_id AS ListingId,
                    r.ad_no AS AdNo,
                    l.title AS ListingTitle,
                    l.status AS ListingStatus,
                    l.cover_image_url AS CoverImageUrl,
                    r.reason_code AS ReasonCode,
                    r.message AS Message,
                    r.status AS Status,
                    r.admin_notes AS AdminNotes,
                    r.reporter_account_id AS ReporterAccountId,
                    TRIM(CONCAT(COALESCE(a.first_name, ''), ' ', COALESCE(a.last_name, ''))) AS ReporterName,
                    a.email AS ReporterEmail,
                    r.reviewed_by_account_id AS ReviewedByAccountId,
                    r.created_at AS CreatedAt,
                    r.reviewed_at AS ReviewedAt
                FROM listing_reports r
                INNER JOIN listings l ON l.id = r.listing_id
                INNER JOIN accounts a ON a.id = r.reporter_account_id
                WHERE r.id = @Id
                """,
                new { Id = id },
                cancellationToken: cancellationToken));
    }

    public async Task<IReadOnlyList<ListingReportListItemDto>> ListAsync(
        string status,
        int take,
        CancellationToken cancellationToken)
    {
        await using var connection = (System.Data.Common.DbConnection)
            await connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        var rows = await connection.QueryAsync<ListingReportListItemDto>(
            new CommandDefinition(
                """
                SELECT
                    r.id AS Id,
                    r.listing_id AS ListingId,
                    r.ad_no AS AdNo,
                    l.title AS ListingTitle,
                    l.status AS ListingStatus,
                    r.reason_code AS ReasonCode,
                    r.message AS Message,
                    r.status AS Status,
                    r.reporter_account_id AS ReporterAccountId,
                    TRIM(CONCAT(COALESCE(a.first_name, ''), ' ', COALESCE(a.last_name, ''))) AS ReporterName,
                    a.email AS ReporterEmail,
                    r.created_at AS CreatedAt,
                    r.reviewed_at AS ReviewedAt
                FROM listing_reports r
                INNER JOIN listings l ON l.id = r.listing_id
                INNER JOIN accounts a ON a.id = r.reporter_account_id
                WHERE r.status = @Status
                ORDER BY r.created_at DESC
                LIMIT @Take
                """,
                new { Status = status, Take = take },
                cancellationToken: cancellationToken));

        return rows.ToArray();
    }

    public async Task<ListingReportCountsDto> GetCountsAsync(CancellationToken cancellationToken)
    {
        await using var connection = (System.Data.Common.DbConnection)
            await connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        var row = await connection.QuerySingleAsync<(int Open, int Reviewing, int Actioned, int Dismissed)>(
            new CommandDefinition(
                """
                SELECT
                    COUNT(*) FILTER (WHERE status = @Open)::int AS Open,
                    COUNT(*) FILTER (WHERE status = @Reviewing)::int AS Reviewing,
                    COUNT(*) FILTER (WHERE status = @Actioned)::int AS Actioned,
                    COUNT(*) FILTER (WHERE status = @Dismissed)::int AS Dismissed
                FROM listing_reports
                """,
                new
                {
                    Open = ListingReportStatus.Open,
                    Reviewing = ListingReportStatus.Reviewing,
                    Actioned = ListingReportStatus.Actioned,
                    Dismissed = ListingReportStatus.Dismissed
                },
                cancellationToken: cancellationToken));

        return new ListingReportCountsDto
        {
            Open = row.Open,
            Reviewing = row.Reviewing,
            Actioned = row.Actioned,
            Dismissed = row.Dismissed
        };
    }

    public async Task<bool> UpdateStatusAsync(
        long id,
        string status,
        long adminAccountId,
        string? adminNotes,
        CancellationToken cancellationToken)
    {
        await using var connection = (System.Data.Common.DbConnection)
            await connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        var affected = await connection.ExecuteAsync(
            new CommandDefinition(
                """
                UPDATE listing_reports
                SET status = @Status,
                    admin_notes = @AdminNotes,
                    reviewed_by_account_id = @AdminAccountId,
                    reviewed_at = NOW()
                WHERE id = @Id
                """,
                new
                {
                    Id = id,
                    Status = status,
                    AdminNotes = adminNotes,
                    AdminAccountId = adminAccountId
                },
                cancellationToken: cancellationToken));

        return affected > 0;
    }
}
