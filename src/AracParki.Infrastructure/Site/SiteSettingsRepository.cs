using AracParki.Application.Abstractions;
using AracParki.Application.Site;
using AracParki.Application.Site.Dtos;
using Dapper;

namespace AracParki.Infrastructure.Site;

public sealed class SiteSettingsRepository(IDbConnectionFactory connectionFactory) : ISiteSettingsStore
{
    private const string SelectSql = """
        SELECT support_email AS SupportEmail,
               support_phone AS SupportPhone,
               whatsapp_phone AS WhatsAppPhone,
               ads_email AS AdsEmail,
               working_hours AS WorkingHours,
               response_note AS ResponseNote,
               company_display_name AS CompanyDisplayName,
               legal_company_name AS LegalCompanyName,
               address_line AS AddressLine,
               city AS City,
               postal_code AS PostalCode,
               footer_tagline AS FooterTagline,
               instagram_url AS InstagramUrl,
               facebook_url AS FacebookUrl,
               twitter_url AS TwitterUrl,
               youtube_url AS YoutubeUrl,
               linkedin_url AS LinkedInUrl,
               tiktok_url AS TikTokUrl,
               updated_at AS UpdatedAt
        FROM site_settings
        WHERE id = 1
        LIMIT 1
        """;

    public async Task<SiteSettingsDto> GetAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = (System.Data.Common.DbConnection)await connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var row = await connection.QuerySingleOrDefaultAsync<SiteSettingsDto>(
            new CommandDefinition(SelectSql, cancellationToken: cancellationToken));
        return row ?? SiteSettingsDto.CreateDefaults();
    }

    public async Task UpdateAsync(SiteSettingsDto settings, long? updatedByAccountId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO site_settings (
                id,
                support_email,
                support_phone,
                whatsapp_phone,
                ads_email,
                working_hours,
                response_note,
                company_display_name,
                legal_company_name,
                address_line,
                city,
                postal_code,
                footer_tagline,
                instagram_url,
                facebook_url,
                twitter_url,
                youtube_url,
                linkedin_url,
                tiktok_url,
                updated_at,
                updated_by_account_id
            ) VALUES (
                1,
                @SupportEmail,
                @SupportPhone,
                @WhatsAppPhone,
                @AdsEmail,
                @WorkingHours,
                @ResponseNote,
                @CompanyDisplayName,
                @LegalCompanyName,
                @AddressLine,
                @City,
                @PostalCode,
                @FooterTagline,
                @InstagramUrl,
                @FacebookUrl,
                @TwitterUrl,
                @YoutubeUrl,
                @LinkedInUrl,
                @TikTokUrl,
                NOW(),
                @UpdatedByAccountId
            )
            ON CONFLICT (id) DO UPDATE SET
                support_email = EXCLUDED.support_email,
                support_phone = EXCLUDED.support_phone,
                whatsapp_phone = EXCLUDED.whatsapp_phone,
                ads_email = EXCLUDED.ads_email,
                working_hours = EXCLUDED.working_hours,
                response_note = EXCLUDED.response_note,
                company_display_name = EXCLUDED.company_display_name,
                legal_company_name = EXCLUDED.legal_company_name,
                address_line = EXCLUDED.address_line,
                city = EXCLUDED.city,
                postal_code = EXCLUDED.postal_code,
                footer_tagline = EXCLUDED.footer_tagline,
                instagram_url = EXCLUDED.instagram_url,
                facebook_url = EXCLUDED.facebook_url,
                twitter_url = EXCLUDED.twitter_url,
                youtube_url = EXCLUDED.youtube_url,
                linkedin_url = EXCLUDED.linkedin_url,
                tiktok_url = EXCLUDED.tiktok_url,
                updated_at = NOW(),
                updated_by_account_id = EXCLUDED.updated_by_account_id
            """;

        await using var connection = (System.Data.Common.DbConnection)await connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(
            new CommandDefinition(
                sql,
                new
                {
                    settings.SupportEmail,
                    settings.SupportPhone,
                    settings.WhatsAppPhone,
                    settings.AdsEmail,
                    settings.WorkingHours,
                    settings.ResponseNote,
                    settings.CompanyDisplayName,
                    settings.LegalCompanyName,
                    settings.AddressLine,
                    settings.City,
                    settings.PostalCode,
                    settings.FooterTagline,
                    settings.InstagramUrl,
                    settings.FacebookUrl,
                    settings.TwitterUrl,
                    settings.YoutubeUrl,
                    settings.LinkedInUrl,
                    settings.TikTokUrl,
                    UpdatedByAccountId = updatedByAccountId
                },
                cancellationToken: cancellationToken));
    }
}
