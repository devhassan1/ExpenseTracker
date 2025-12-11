using Microsoft.Extensions.DependencyInjection;
using ExpenseTracker.Application.UseCases.Expenses;
using ExpenseTracker.Application.UseCases.Reports;

namespace ExpenseTracker.Application;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<AddExpenseUseCase>();
        services.AddScoped<ListExpensesUseCase>();
        services.AddScoped<ExportExpensesUseCase>();

        return services;
    }
}
