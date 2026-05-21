using FinanceTracker.Api.Application.DTOs;

namespace FinanceTracker.Api.Domain;

public interface ITransactionRepository
{
    Task<List<Transaction>> GetTransactions(int accountId);
    Task<Transaction> Create(int accountId, decimal amount, string description, string? category, DateTime? date);
    Task<Transaction?> GetTransactionById(int accountId);
    Task<bool> Delete(int accountId, int transactionId, int userId);
    Task<int> GetCount(int accountId, int userId);
    Task<List<Transaction>> GetPaged(int accountId, int userId, int page, int pageSize);
    Task<Transaction?> Update(int accountId, int transactionId, int userId, UpdateTransactionRequest request);
}