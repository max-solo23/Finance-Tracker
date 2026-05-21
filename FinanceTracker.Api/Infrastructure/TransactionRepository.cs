using FinanceTracker.Api.Application.DTOs;
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
    
    public async Task<Transaction> Create(
        int accountId, decimal amount, string description, string? category, DateTime? date)
    {
        var account = await _context.Accounts.FindAsync(accountId);

        if (account == null)
        {
            throw new InvalidOperationException($"Account {accountId} not found.");
        }

        var resolvedDate = date.HasValue
            ? DateTime.SpecifyKind(date.Value, DateTimeKind.Utc)
            : DateTime.UtcNow;

        var transaction = new Transaction(amount, description, resolvedDate);

        transaction.Category = category;

        account.AddTransaction(transaction);

        await _context.SaveChangesAsync();

        return transaction;
    }

    public async Task<bool> Delete(int accountId, int transactionId, int userId)
    {
        var transactionToDelete = await _context.Transactions
            .Where(t => t.Id == transactionId && t.AccountId == accountId)
            .Where(t => _context.Accounts.Any(
                account => account.Id == accountId && account.UserId == userId))
            .FirstOrDefaultAsync();

        if (transactionToDelete == null) return false;

        _context.Transactions.Remove(transactionToDelete);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<Transaction?> Update(
        int accountId, int transactionId, int userId, UpdateTransactionRequest request)
    {
        var transactionToUpdate = await _context.Transactions
            .Where(t => t.Id == transactionId && t.AccountId == accountId)
            .Where(t => _context.Accounts.Any(
                account => account.Id == accountId && account.UserId == userId))
            .FirstOrDefaultAsync();

        if (transactionToUpdate == null) return null;

        if (request.Amount != null) transactionToUpdate.Amount = request.Amount.Value;
        if (request.Description != null) transactionToUpdate.Description = request.Description;
        if (request.Date != null) transactionToUpdate.Date = DateTime.SpecifyKind(request.Date.Value, DateTimeKind.Utc);
        if (request.Category != null) transactionToUpdate.Category = request.Category;

        await _context.SaveChangesAsync();
        return transactionToUpdate;
    }

    public async Task<int> GetCount(int accountId, int userId)
    {
        return await _context.Transactions
            .Where(t => t.AccountId == accountId)
            .Where(t => _context.Accounts.Any(
                a => a.Id == accountId && a.UserId == userId
            ))
            .CountAsync();
    }

    public async Task<List<Transaction>> GetPaged(int accountId, int userId, int page, int pageSize)
    {
        return await _context.Transactions
            .Where(t => t.AccountId == accountId)
            .Where(t => _context.Accounts.Any(
                a => a.Id == accountId && a.UserId == userId
            ))
            .OrderBy(t => t.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
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