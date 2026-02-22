using FinanceTracker.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Api.Infrastructure;

public class TransactionRepository : ITransactionRepository
{
    private readonly FinanceTrackerContext _context;

    public TransactionRepository(FinanceTrackerContext financeTrackerContext)
    {
        _context = financeTrackerContext;
    }
    
    public async Task<Transaction> Create(int accountId, decimal amount, string description)
    {
        var account = await _context.Accounts.FindAsync(accountId);

        if (account == null)
        {
            throw new InvalidOperationException($"Account {accountId} not found.");
        }

        var transaction = new Transaction(amount, description, DateTime.UtcNow);

        account.AddTransaction(transaction);

        await _context.SaveChangesAsync();

        return transaction;
    }

    public async Task<Transaction?> GetTransactionById(int id)
    {
        var transaction = await _context.Transactions.FindAsync(id);

        return transaction;
    }

    public async Task<List<Transaction>> GetTransactions(int accountId)
    {
        var account = await _context.Accounts
            .Include(account => account.Transactions)
            .FirstOrDefaultAsync(account => account.Id == accountId);

        if (account == null)
        {
            throw new InvalidOperationException($"Account {accountId} not found");
        }

        var transactions = account.Transactions;

        return transactions;
    }
}