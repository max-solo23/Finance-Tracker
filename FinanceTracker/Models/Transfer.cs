namespace FinanceTracker.Models;

public class Transfer
{
    public int Id { get; set; }
    public int FromAccountId { get; set; }
    public int ToAccountId { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime ProcessAt { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;

    public Account? FromAccount { get; set; }
    public Account? ToAccount { get; set; }
}