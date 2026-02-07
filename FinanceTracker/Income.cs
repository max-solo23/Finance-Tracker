public class Income : Transaction
{
    public string Source { get; set; } = string.Empty;

    public Income(decimal amount, string description, DateTime date, string source)
        : base(amount, description, date)
    {
        Source = source;
    }
}