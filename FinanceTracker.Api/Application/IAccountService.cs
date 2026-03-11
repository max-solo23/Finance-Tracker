namespace FinanceTracker.Api.Application;

public interface IAccountService
{
    Task<IEnumerable<Account>> GetAll(int userId);
    Task<Account?> GetById(int id, int userId);
    Task<Account> Create(string name, int userId);
    Task<Account?> Update(int id, string name, int userId);
    Task<bool> Delete(int id, int userId);
    Task<List<int>> ExistsByIds(List<int> ids, int userId);
}