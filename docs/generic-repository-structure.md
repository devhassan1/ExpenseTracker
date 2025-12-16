Goal: Align project folder layout with a standard Generic Repository + UnitOfWork pattern

Recommended folder structure (relative to solution root):

- ExpenseTracker.Application
  - Interfaces
    - IRepository.cs                // generic repository contract
    - IUnitOfWork.cs                // unit of work contract
    - Repositories\                 // domain-specific repository interfaces (e.g., IExpenseRepository, IUserRepository, ITagRepository)

- ExpenseTracker.Domain
  - Entities\                      // domain entities (User, Expense, Tag, Category, Role, etc.)

- ExpenseTracker.Infrastructure
  - Persistence\                   // OracleDbContext and EF model configuration
  - Repositories\                  // concrete repositories
    - Generic\                     // generic EF repository implementation (EfRepository&lt;T&gt;)
    - ExpenseRepository.cs
    - TagRepository.cs
    - UserRepository.cs
  - UnitOfWork\                    // EfUnitOfWork implementation
  - Migrations\                    // if using EF migrations

- ExpenseTracker.Web
  - Controllers\
  - Facades\
  - wwwroot\

Guidelines / actions taken in this repo
- Kept existing implementations but created a small "Generic" folder placeholder under Infrastructure/Repositories for the `EfRepository<T>` implementation.
- Confirmed `EfUnitOfWork` lives under `Infrastructure/UnitOfWork` and `IUnitOfWork` is in Application/Interfaces.
- Repositories no longer call SaveChanges; UnitOfWork handles persistence.

What you should do next to fully align the code:
1. Move any remaining concrete repository files into `Infrastructure/Repositories` if they are elsewhere.
2. Move `EfRepository.cs` into `Infrastructure/Repositories/Generic` (if you prefer) and update its namespace to `ExpenseTracker.Infrastructure.Repositories.Generic`.
3. Keep domain-specific repository interfaces in `ExpenseTracker.Application/Interfaces/Repositories`.
4. Ensure DI registrations are consistent (register `IRepository<>` => `EfRepository<>`, register `IUnitOfWork` => `EfUnitOfWork`). This repository already registers these.
5. Adjust `using`/namespaces in files after any file moves.

Notes
- This document is intentionally conservative. I created placeholders so the folder tree exists and to make it clear where to move code. If you want, I can move files and update namespaces automatically — tell me which files to move and I will update namespaces and DI registrations accordingly.

