using ExpenseTracker.Application.Interfaces.Common;
using ExpenseTracker.Application.Interfaces.Exports;
using ExpenseTracker.Application.Interfaces.Repositories;
using ExpenseTracker.Application.UseCases;
using ExpenseTracker.Domain.Repositories;
using ExpenseTracker.Infrastructure.Auth;
using ExpenseTracker.Infrastructure.Exports;
using ExpenseTracker.Infrastructure.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ExpenseTracker.Domain;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        // Repositories
        services.AddScoped<IExpenseService, ExpenseRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();

        services.AddScoped<ITagRepository, TagRepository>();
        services.AddScoped<ITagService, TagService>();
        services.AddScoped<IUserRepository, UserRepository>();

        // Cross-cutting

        services.AddScoped<IExpenseExporter, PdfExpenseExporter>();
        services.AddScoped<IExpenseExporter, CsvExpenseExporter>();
        services.AddScoped<IExpenseExporter, XlsxExpenseExporter>();


        // Users
        services.AddScoped<IUserRepository, UserRepository>();

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Generic EF repository
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        // User context
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();

        return services;
    }
}
