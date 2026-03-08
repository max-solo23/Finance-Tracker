using FinanceTracker.Api.Domain;
using FinanceTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Api.Infrastructure;

public class TransferRepository : ITransferRepository
{
    private readonly FinanceTrackerContext _context;

    public TransferRepository(FinanceTrackerContext financeTrackerContext)
    {
        _context = financeTrackerContext;
    }

    public async Task<Transfer> Create(int fromAccountId, int toAccountId, decimal amount, string description, string idempotencyKey)
    {
        var transfer = new Transfer
        {
            FromAccountId = fromAccountId, 
            ToAccountId = toAccountId, 
            Amount = amount, 
            Description = description, 
            ProcessAt = DateTime.UtcNow,
            IdempotencyKey = idempotencyKey 
        };

        var fromAccount = await _context.Accounts.FindAsync(fromAccountId);
        fromAccount!.AddTransaction(-transfer.Amount, transfer.Description, transfer.ProcessAt);

        var toAccount = await _context.Accounts.FindAsync(toAccountId);
        toAccount!.AddTransaction(transfer.Amount, transfer.Description, transfer.ProcessAt);

        try
        {
            await _context.Transfers.AddAsync(transfer);
            await _context.SaveChangesAsync();
            return transfer;
        }
        catch (DbUpdateException)
        {
            var existingTransfer = await _context.Transfers
                .FirstOrDefaultAsync(t => t.IdempotencyKey == transfer.IdempotencyKey);

            return existingTransfer!;            
        }
    }

    public async Task<Transfer?> GetByIdempotencyKey(string idempotencyKey)
    {
        var transfer = await _context.Transfers
            .FirstOrDefaultAsync(transfer => transfer.IdempotencyKey == idempotencyKey);
        
        return transfer;
    }
}