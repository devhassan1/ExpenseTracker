
using ExpenseTracker.Application.DTOs;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Application.Interfaces.Repositories;
using ExpenseTracker.Common.Results;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.ValueObjects;

namespace ExpenseTracker.Application.UseCases.Expenses;

public sealed class AddExpenseUseCase
{
    private readonly IExpenseRepository _expenses;
    private readonly ICategoryService _categories;
    private readonly ITagRepository _tags;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;

    public AddExpenseUseCase(
        IExpenseRepository expenses,
        ICategoryService categories,
        ITagRepository tags,
        ICurrentUser currentUser,
        IClock clock)
    {
        _expenses = expenses;
        _categories = categories;
        _tags = tags;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<Result<long>> ExecuteAsync(AddExpenseRequest req, CancellationToken ct)
    {
        var expense = new Expense
        {
            UserId = _currentUser.UserId,
            Description = req.Description,
            TxnDate = req.TxnDate,
            CreatedAt = _clock.UtcNow
        };

        expense.SetMoney(new Money(req.Amount));

        var id = await _expenses.CreateAsync(expense, ct);

        // Tag selected from dropdown
        if (req.TagId is long tagId)
            await _tags.AssignToExpenseAsync(id, tagId, ct);

        return Result<long>.Success(id);
    }
}
