public class Expense : Transaction
{
    public string Category { get; set; } = string.Empty;

    public Expense(decimal amount, string description, DateTime date, string category)
        : base(amount, description, date)
    {
        Category = category;
    }
}