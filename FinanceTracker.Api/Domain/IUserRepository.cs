using FinanceTracker.Models;

namespace FinanceTracker.Api.Domain;

public interface IUserRepository
{
    Task<User?> GetByEmail(string email);
    Task<bool> ExistsByEmail(string email);
    Task<User> Create(string email, string passwordHash);
}