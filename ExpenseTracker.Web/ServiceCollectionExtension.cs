using Microsoft.Extensions.DependencyInjection;
using ExpenseTracker.Web.Facades;

namespace ExpenseTracker.Web;

public static class FacadeServiceCollectionExtensions
{
    public static IServiceCollection AddFacades(this IServiceCollection services)
    {
        services.AddScoped<ExpenseFacade>();
        services.AddScoped<ReportFacade>();

        return services;
    }
}

