using FinanceTracker.Api.Domain;
using FinanceTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Api.Infrastructure;

public class UserRepository : IUserRepository
{
    private readonly FinanceTrackerContext _context;
    public UserRepository(FinanceTrackerContext financeTrackerContext)
    {
        _context = financeTrackerContext;
    }
    public async Task<User> Create(string email, string passwordHash)
    {
        var user = new User
        {
            Email = email,
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return user;
    }

    public async Task<bool> ExistsByEmail(string email)
    {
        return await _context.Users.AnyAsync(user => user.Email == email);
    }

    public async Task<User?> GetByEmail(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(user => user.Email == email);
    }
}