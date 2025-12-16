using ExpenseTracker.Domain.ValueObjects;

namespace ExpenseTracker.Domain.Entities
{
    public class Expense
    {
        public long Id { get; set; }
        public long UserId { get; set; }             // owner
        public Money Money { get; private set; } = new(0);
        public string? Description { get; set; }
        public DateTime TxnDate { get; set; }        // UTC recommended
        public DateTime CreatedAt { get; set; }

        public void SetMoney(Money money) => Money = money;
    }

}
