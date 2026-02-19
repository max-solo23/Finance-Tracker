namespace FinanceTracker.Api.Domain;

public interface IAccountRepository
{
    Task<Account?> GetById(int id);
    Task<List<Account>> GetAll();
    Task<Account> Create(string name);
    Task<Account?> Update(int id, string name);
    Task Delete(int id);
    Task<List<bool>> ExistsByIds(List<int> ids);
}