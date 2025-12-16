using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpenseTracker.Application.DTOs
{

    public sealed record NewUserRequest(string Name, string? Email, string? Password);

    public sealed record AddExpenseRequest(
        decimal Amount,
        string? Description,
        DateTime TxnDate,
        long? ForUserId,     // optional: admin creating for user
        long[]? TagIds,
        NewUserRequest? NewUser
    );

    public record ExpenseFilterRequest(DateTime From, DateTime To, long? ForUserId)
    {
        public ExpenseFilterRequest(DateTime from, DateTime to)
            : this(from, to, null)
        {
        }
        public ExpenseFilterRequest(DateOnly? from, DateOnly? to, long? forUserId)
    : this(
        from?.ToDateTime(TimeOnly.MinValue) ?? DateTime.MinValue,
        to?.ToDateTime(TimeOnly.MaxValue) ?? DateTime.MaxValue,
        forUserId)
        { }
    }

    public sealed class ExportRequest
    {
        public DateTime From { get; init; }
        public DateTime To { get; init; }
        public long? ForUserId { get; init; }
        public string Format { get; init; } = "csv";

        public ExportRequest() { }

        public ExportRequest(DateTime from, DateTime to, long? forUserId, string format)
        {
            From = from;
            To = to;
            ForUserId = forUserId;
            Format = format;
        }
        public ExportRequest(DateOnly? from, DateOnly? to, long? forUserId, string format)
    : this(
        from?.ToDateTime(TimeOnly.MinValue) ?? DateTime.MinValue,
        to?.ToDateTime(TimeOnly.MaxValue) ?? DateTime.MaxValue,
        forUserId,
        format)
        { }
    }

}
