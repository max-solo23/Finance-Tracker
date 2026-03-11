using FinanceTracker.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Api.Infrastructure;

public class AccountRepository : IAccountRepository
{
    private readonly FinanceTrackerContext _context;

    public AccountRepository(FinanceTrackerContext financeTrackerContext)
    {
        _context = financeTrackerContext;
    }

    public async Task<Account> Create(string name, int userId)
    {
        var account = new Account(name);

        account.UserId = userId;

        _context.Accounts.Add(account);

        await _context.SaveChangesAsync();

        return account;
    }

    public async Task<bool> Delete(int id, int userId)
    {
        var account = await _context.Accounts
            .FirstOrDefaultAsync(account => account.Id == id && account.UserId == userId);

        if (account == null) return false;

        _context.Accounts.Remove(account);

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<List<int>> ExistsByIds(List<int> ids, int userId)
    {
        var existingIds = await _context.Accounts
            .Where(account => ids.Contains(account.Id) && account.UserId == userId)
            .Select(account => account.Id)
            .ToListAsync();

        return existingIds;
    }

    public async Task<List<Account>> GetAll(int userId)
    {
        var accounts = await _context.Accounts
            .Where(account => account.UserId == userId)
            .ToListAsync();

        return accounts;
    }

    public async Task<Account?> GetById(int id, int userId)
    {
        var account = await _context.Accounts
            .Include(account => account.Transactions)
            .FirstOrDefaultAsync(account => account.Id == id && account.UserId == userId);

        return account;
    }

    public async Task<Account?> Update(int id, string name, int userId)
    {
        var account = await _context.Accounts
            .FirstOrDefaultAsync(account => account.Id == id && account.UserId == userId);

        if (account == null) return null;

        account.Name = name;

        await _context.SaveChangesAsync();

        return account;
    }
}