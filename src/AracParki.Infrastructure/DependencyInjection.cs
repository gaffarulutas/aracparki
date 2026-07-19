using AracParki.Application.Abstractions;
using AracParki.Application.Accounts;
using AracParki.Application.Catalog;
using AracParki.Application.Listings;
using AracParki.Infrastructure.Accounts;
using AracParki.Infrastructure.Catalog;
using AracParki.Infrastructure.Listings;
using AracParki.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace AracParki.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<ISqlQueryLoader, SqlQueryLoader>();
        services.AddSingleton<IDbConnectionFactory, NpgsqlConnectionFactory>();
        services.AddScoped<IListingQuery, ListingRepository>();
        services.AddScoped<ICatalogQuery, CatalogRepository>();
        services.AddScoped<IAccountStore, AccountRepository>();
        return services;
    }
}
