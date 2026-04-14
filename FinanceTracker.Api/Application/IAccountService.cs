using FinanceTracker.Api.Application.DTOs;

namespace FinanceTracker.Api.Application;

public interface IAccountService
{
    Task<PagedResponse<AccountSummaryDto>> GetAll(int userId, int page, int pageSize);
    Task<Account?> GetById(int id, int userId);
    Task<Account> Create(string name, int userId);
    Task<Account?> Update(int id, string name, int userId);
    Task<bool> Delete(int id, int userId);
    Task<List<int>> ExistsByIds(List<int> ids, int userId);
}