namespace FinanceTracker.Models;

public class RecurringSchedule
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int AccountId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int IntervalMonths { get; set; }
    public DateTime AnchorDate { get; set; }
    public string? Category { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public RecurringSchedule(
        int userId, 
        int accountId, 
        string name, 
        string description, 
        decimal amount, 
        int intervalMonths, 
        DateTime anchorDate, 
        string? category)
    {
        UserId = userId;
        AccountId = accountId;
        Name = name;
        Description = description;
        Amount = amount;
        IntervalMonths = intervalMonths;
        AnchorDate = DateTime.SpecifyKind(anchorDate, DateTimeKind.Utc);
        Category = category;
    }

}