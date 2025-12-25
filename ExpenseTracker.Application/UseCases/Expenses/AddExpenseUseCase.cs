
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

    public AddExpenseUseCase(
    IExpenseService expenses,
    ICategoryRepository categories,
    ITagRepository tags,
    IUserRepository users,
    IUnitOfWork uow,
    ICurrentUser currentUser)
    {
        _expenses = expenses;
        _categories = categories;
        _tags = tags;
        _users = users;
        _uow = uow;
        _currentUser = currentUser;
    }
    public async Task<Result<long>> ExecuteAsync(AddExpenseRequest req, CancellationToken ct)
    {
        var expense = new Expense
        {
            UserId = req.ForUserId ?? _currentUser.UserId,
            Description = req.Description,
            TxnDate = req.TxnDate,
            CreatedAt = DateTime.UtcNow
        };

        expense.SetMoney(new Money(req.Amount));

        long createdExpenseId = 0;


        await _uow.ExecuteInTransaction(async () =>
        {
            // (Optional) If Admin creates a new user inline, persist the user first to satisfy FK on expense
            if (req is not null && req is { NewUser: not null } && (_currentUser.Role.Contains("Admin") || _currentUser.Role.Contains("SuperAdmin")))
            {
                var nu = req.NewUser!;
                var newUser = new Domain.Entities.User
                {
                    Name = nu.Name,
                    Email = nu.Email ?? string.Empty,
                    PasswordHash = nu.Password ?? string.Empty, // TODO: hash in production
                    parent_user_id = _currentUser.UserId,
                    RoleId = 3 // default User
                };

                // Create user row and flush to DB to obtain real Id (FK parent for expense)
                var createdUserId = await _users.Create(newUser, ct);
                await _uow.SaveChanges(ct);

                // Now we have a real parent user row. Use it on the expense.
                expense.UserId = createdUserId;

                // optional note
                expense.Description = (expense.Description ?? string.Empty) + $"\n[Created user: {newUser.Name} (id={createdUserId})]";
            }

            // 1) Create the expense (add to DbContext)
            await _expenses.Create(expense, ct);

            // 2) Flush to DB so we get a real Expense.Id (principal exists)
            await _uow.SaveChanges(ct);

            // 3) Assign tags using expense.Id (now a real FK value)
            if (req.TagIds is { Length: > 0 } tagIds)
            {
                foreach (var tId in tagIds)
                {
                    if (tId <= 0) continue;
                    await _tags.AssignToExpense(expense.Id, tId, ct);
                }
            }

            // 4) Final save to persist EXPENSE_TAG rows
            await _uow.SaveChanges(ct);

            createdExpenseId = expense.Id; // populated after the first SaveChanges
        }, ct);

        return Result<long>.Success(createdExpenseId);
    }
}