using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PhilCharronWebFolio.Application.Common.Interfaces;
using PhilCharronWebFolio.Infrastructure.Persistence;
using PhilCharronWebFolio.Infrastructure.Persistence.Interceptors;
using PhilCharronWebFolio.Infrastructure.Persistence.Repositories;
using PhilCharronWebFolio.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace PhilCharronWebFolio.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. Indispensable pour accéder au contexte HTTP (User, Claims, etc.)
        services.AddHttpContextAccessor();

        // 2. Enregistrement des services
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<AuditableEntityInterceptor>();

        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<ITokenService, TokenService>();

        // 3. Ton nouveau service de Bug Report
        //services.AddScoped<IBugReportService, BugReportService>();

        // 4. Repos
        services.AddScoped<IBugReportRepository, BugReportRepository>();
        services.AddScoped<IAccessibilityAuditRepository, AccessibilityAuditRepository>();
        services.AddScoped<IContactMessageRepository, ContactMessageRepository>();

        // Ajoute ceci :
        /*
        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<ApplicationDbContext>());
        */

        return services;
    }
}
