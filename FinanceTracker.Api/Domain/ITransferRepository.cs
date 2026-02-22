using FinanceTracker.Models;

namespace FinanceTracker.Api.Domain;

public interface ITransferRepository
{
    Task<Transfer?> GetByIdempotencyKey(string idempotencyKey);
    Task<Transfer> Create(int fromAccountId, int toAccountId, decimal amount, string description, string idempotencyKey);
}