namespace FinanceTracker.Api.Domain;

public interface ITransactionRepository
{
    Task<List<Transaction>> GetTransactions(int accountId);
    Task<Transaction> Create(int accountId, decimal amount, string description, string? category);
    Task<Transaction?> GetTransactionById(int accountId);
    Task<bool> Delete(int accountId, int transactionId, int userId);
}