namespace FinanceTracker.Api.Application;

public interface IAccountService
{
    Task<IEnumerable<Account>> GetAll();
    Task<Account?> GetById(int id);
    Task<Account> Create(string name);
    Task<Account?> Update(int id, string name);
    Task<bool> Delete(int id);
    Task<List<int>> ExistsByIds(List<int> ids);
}