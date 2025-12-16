
using ExpenseTracker.Application.DTOs;
using ExpenseTracker.Application.Interfaces.Common;
using ExpenseTracker.Application.Interfaces.Repositories;
using ExpenseTracker.Common.Results;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.ValueObjects;

namespace ExpenseTracker.Application.UseCases.Expenses;

public sealed class AddExpenseUseCase
{
    private readonly IExpenseService _expenses;
    private readonly ICategoryRepository _categories;
    private readonly ITagRepository _tags;
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;

        public AddExpenseUseCase(
        IExpenseService expenses,
        ICategoryRepository categories,
        ITagRepository tags,
        IUserRepository users,
        IUnitOfWork uow,
        ICurrentUser currentUser,
        IClock clock)
    {
        _expenses = expenses;
        _categories = categories;
        _tags = tags;
            _users = users;
        _uow = uow;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<Result<long>> ExecuteAsync(AddExpenseRequest req, CancellationToken ct)
    {
        var expense = new Expense
        {
            // If admin creates for another user, use that; otherwise current user
            UserId = req.ForUserId ?? _currentUser.UserId,
            Description = req.Description,
            TxnDate = req.TxnDate,
            CreatedAt = DateTime.UtcNow // _clock.UtcNow
        };

        expense.SetMoney(new Money(req.Amount));

        // Perform the entire operation in a transaction
        long createdExpenseId = 0;
        await _uow.ExecuteInTransaction(async () =>
        {
            // If admin provided a new user payload, create the user and set parent_user_id
            if (req is not null && req is { NewUser: not null } && _currentUser.Role.Contains("Admin"))
            {
                var nu = req.NewUser!;
                var newUser = new Domain.Entities.User
                {
                    Name = nu.Name,
                    Email = nu.Email ?? string.Empty,
                    PasswordHash = nu.Password ?? string.Empty,
                    parent_user_id = _currentUser.UserId,
                    RoleId = 3 // default User
                };

                var createdUserId = await _users.Create(newUser, ct);
                expense.UserId = createdUserId;

                // append note
                var note = $"\n[Created user: {newUser.Name} (id={createdUserId})]";
                expense.Description = (expense.Description ?? string.Empty) + note;
            }

            var id = await _expenses.Create(expense, ct);
            createdExpenseId = id;

            // Tags selected from dropdown (may be multiple)
            if (req.TagIds is { Length: > 0 } tagIds)
            {
                foreach (var tId in tagIds)
                {
                    // ignore invalid ids (<= 0)
                    if (tId <= 0) continue;
                    await _tags.AssignToExpense(id, tId, ct);
                }
            }

            // UnitOfWork will save at the end of ExecuteInTransactionAsync
        }, ct);

        return Result<long>.Success(createdExpenseId);
    }
}
