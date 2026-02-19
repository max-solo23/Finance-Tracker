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

    public async Task<Account> Create(string name)
    {
        var account = new Account(name);

        _context.Accounts.Add(account);

        await _context.SaveChangesAsync();

        return account;
    }

    public async Task Delete(int id)
    {
        var account = await _context.Accounts.FindAsync(id);

        if (account == null)
        {
            return;
        }

        _context.Accounts.Remove(account);

        await _context.SaveChangesAsync();

        return;
    }

    public async Task<List<int>> ExistsByIds(List<int> ids)
    {
        var existingIds = await _context.Accounts
            .Where(account => ids.Contains(account.Id))
            .Select(account => account.Id)
            .ToListAsync();

        return existingIds;
    }

    public async Task<List<Account>> GetAll()
    {
        var accounts = await _context.Accounts.ToListAsync();

        return accounts;
    }

    public async Task<Account?> GetById(int id)
    {
        var account = await _context.Accounts
            .Include(account => account.Transactions)
            .FirstOrDefaultAsync(account => account.Id == id);

        return account;
    }

    public async Task<Account?> Update(int id, string name)
    {
        var account = await _context.Accounts.FindAsync(id);

        if (account == null)
        {
            return null;
        }

        account.Name = name;

        await _context.SaveChangesAsync();

        return account;
    }
}