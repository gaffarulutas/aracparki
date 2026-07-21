using AracParki.Application.Accounts;
using AracParki.Application.Accounts.Services;
using AracParki.Application.Catalog.Services;
using AracParki.Application.Listings;
using AracParki.Application.Listings.Services;
using AracParki.Application.Listings.Validation;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace AracParki.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<ListingSearchQueryValidator>();
        services.AddSingleton<ListingImageUrlPolicy>();
        services.AddScoped<ListingService>();
        services.AddScoped<ListingCommandService>();
        services.AddScoped<ListingModerationService>();
        services.AddScoped<CatalogService>();
        services.AddScoped<AuthEmailService>();
        services.AddScoped<AccountService>();
        return services;
    }
}
