public interface IAccount
{
    string Name { get; }
    List<Transaction> Transactions { get; }

    void AddTransaction(decimal amount, string description, DateTime dateTime);
    void AddTransaction(Transaction transaction);
    decimal GetBalance();
    List<Transaction> GetExpenses();
    List<Transaction> GetIncome();
    int GetTransactionCount();
    List<Transaction> GetTransactionsSortedByAmount();
    Transaction? GetMostRecentTransaction();
    bool HasExpenses();
    List<String> GetAllDescriptions();
    void CheckLowBalance(decimal threshold);

    event EventHandler? LowBalanceAlert;

    Transaction this[int index] { get; }
}