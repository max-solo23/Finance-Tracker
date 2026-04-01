public class Transaction
{
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public int Id { get; set; }
    public int AccountId { get; set; }
    public string? Category { get; set; }

    public Transaction(decimal amount, string description, DateTime date)
    {
        Amount = amount;
        Description = description;
        Date = date;
    }
}