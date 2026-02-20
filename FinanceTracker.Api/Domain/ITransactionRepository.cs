namespace FinanceTracker.Api.Domain;

public interface ITransactionRepository
{
    Task<List<Transaction>> GetTransactions(int id);
    Task<Transaction> Create(int accountId, decimal amount, string description);
    Task<Transaction?> GetTransactionById(int accountId);
}