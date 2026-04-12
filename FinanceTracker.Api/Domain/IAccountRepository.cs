namespace FinanceTracker.Api.Domain;

public interface IAccountRepository
{
    Task<Account?> GetById(int id, int userId);
    Task<List<Account>> GetAll(int userId);
    Task<Account> Create(string name, int userId);
    Task<Account?> Update(int id, string name, int userId);
    Task<bool> Delete(int id, int userId);
    Task<List<int>> ExistsByIds(List<int> ids, int userId);
    Task<int> GetCount(int userId);
    Task<List<Account>> GetPaged(int userId, int page, int pageSize);
}