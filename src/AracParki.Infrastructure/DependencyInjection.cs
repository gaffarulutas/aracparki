using AracParki.Application.Abstractions;
using AracParki.Application.Accounts;
using AracParki.Application.Catalog;
using AracParki.Application.Email;
using AracParki.Application.Listings;
using AracParki.Application.Messaging;
using AracParki.Infrastructure.Accounts;
using AracParki.Infrastructure.Catalog;
using AracParki.Infrastructure.Email;
using AracParki.Infrastructure.Listings;
using AracParki.Infrastructure.Messaging;
using AracParki.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AracParki.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<EmailSettings>(configuration.GetSection(EmailSettings.SectionName));
        services.Configure<AppSettings>(configuration.GetSection(AppSettings.SectionName));
        services.Configure<WhatsAppSettings>(configuration.GetSection(WhatsAppSettings.SectionName));

        services.AddMemoryCache();
        services.AddHttpClient(WhatsAppOtpSender.HttpClientName);

        services.AddSingleton<ISqlQueryLoader, SqlQueryLoader>();
        services.AddSingleton<IDbConnectionFactory, NpgsqlConnectionFactory>();
        services.AddSingleton<DatabaseMigrator>();
        services.AddScoped<IListingQuery, ListingRepository>();
        services.AddScoped<IListingStore, ListingWriteRepository>();
        services.AddScoped<IWizardDraftStore, WizardDraftRepository>();
        services.AddScoped<IListingImageStorage, LocalListingImageStorage>();
        services.AddScoped<ICatalogQuery, CatalogRepository>();
        services.AddScoped<IAccountStore, AccountRepository>();
        services.AddScoped<IPhoneOtpStore, PhoneOtpRepository>();
        services.AddScoped<IPhoneOtpService, PhoneOtpService>();
        services.AddScoped<IWhatsAppOtpSender, WhatsAppOtpSender>();
        services.AddScoped<IEmailSender, SmtpEmailSender>();
        return services;
    }
}
