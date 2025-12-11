using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Application.Interfaces.Repositories;
using ExpenseTracker.Application.Interfaces.Services;
using ExpenseTracker.Application.Services;
using ExpenseTracker.Infrastructure.Auth;
using ExpenseTracker.Infrastructure.Exports;
using ExpenseTracker.Infrastructure.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ExpenseTracker.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        // Repositories
        services.AddScoped<IExpenseRepository, ExpenseRepository>();
        services.AddScoped<ICategoryService, CategoryRepository>();

        services.AddScoped<ITagRepository, TagRepository>();
        services.AddScoped<ITagService, TagService>();


        // Cross-cutting
        services.AddSingleton<IClock, SystemClock>();
        services.AddScoped<IExportService, CsvExportService>();

        // User context
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser>(sp =>
        {
            var http = sp.GetRequiredService<IHttpContextAccessor>().HttpContext;
            return new CurrentUser(http?.User ?? new System.Security.Claims.ClaimsPrincipal());
        });

        return services;
    }
}
