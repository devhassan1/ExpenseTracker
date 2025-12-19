using ExpenseTracker.Application.Interfaces.Exports;
using ExpenseTracker.Application.UseCases.Expenses;
using ExpenseTracker.Application.UseCases.Reports;
using Microsoft.Extensions.DependencyInjection;

namespace ExpenseTracker.Application;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<AddExpenseUseCase>();
        services.AddScoped<ListExpensesUseCase>();
        services.AddScoped<ExportExpensesUseCase>();
        services.AddScoped<IExpenseExporterFactory, ExpenseExporterFactory>();
        services.AddScoped<ExportExpensesUseCase>();
        return services;
    }
}
