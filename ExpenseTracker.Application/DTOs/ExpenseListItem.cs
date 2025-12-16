using System;
using System.Collections.Generic;

namespace ExpenseTracker.Application.DTOs
{
    public sealed record TagDto(long Id, string Label);

    public sealed record ExpenseListItem(
        long Id,
        long UserId,
        string? UserName,
        DateTime TxnDate,
        decimal Amount,
        string Currency,
        string? Description,
        TagDto[] Tags
    );
}
