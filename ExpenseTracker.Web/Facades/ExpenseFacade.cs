using ExpenseTracker.Application.DTOs;
using ExpenseTracker.Application.UseCases.Expenses;
using ExpenseTracker.Common.Results;

namespace ExpenseTracker.Web.Facades;

public sealed class ExpenseFacade
{
    private readonly AddExpenseUseCase _add;
    private readonly ListExpensesUseCase _list;

    public ExpenseFacade(AddExpenseUseCase add, ListExpensesUseCase list)
    {
        _add = add;
        _list = list;
    }

    public Task<Result<long>> AddAsync(AddExpenseRequest req, CancellationToken ct)
        => _add.ExecuteAsync(req, ct);

    public async Task<Result<List<ExpenseListItem>>> ListAsync(
        ExpenseFilterRequest filter, int page, int pageSize, string? search, CancellationToken ct)
    {
        return await _list.GetPagedExpensesAsync(filter, page, pageSize, search, ct);
    }
}
