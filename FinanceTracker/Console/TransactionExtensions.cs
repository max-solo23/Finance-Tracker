public static class TransactionExtensions
{
    public static decimal GetTotalExpenses(this List<Transaction> transactions)
    {
        return transactions.Where(t => t.Amount < 0).Sum(t => t.Amount);
    }

    public static List<Transaction> GetLast7Days(this List<Transaction> transactions)
    {
        DateTime cutoff = DateTime.Now.AddDays(-7);
        return transactions.Where(t => t.Date >= cutoff).ToList();
    }
}
