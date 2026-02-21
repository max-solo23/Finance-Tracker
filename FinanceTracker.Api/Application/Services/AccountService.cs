
using FinanceTracker.Api.Domain;

namespace FinanceTracker.Api.Application.Services;

public class AccountService : IAccountService
{
    private readonly IAccountRepository _repository;

    public AccountService(IAccountRepository accountRepository)
    {
        _repository = accountRepository;
    }
    
    public Task<Account> Create(string name)
    {
        var account = _repository.Create(name);

        return account;
    }

    public Task<bool> Delete(int id)
    {
        var result = _repository.Delete(id);

        return result;
    }

    public async Task<List<int>> ExistsByIds(List<int> ids)
    {
        var existingIds = await _repository.ExistsByIds(ids);

        return existingIds;
    }

    public async Task<IEnumerable<Account>> GetAll()
    {
        var accounts = await _repository.GetAll();

        return accounts;
    }

    public Task<Account?> GetById(int id)
    {
        var account = _repository.GetById(id);

        return account;
    }

    public Task<Account?> Update(int id, string name)
    {
        var account = _repository.Update(id, name);

        return account;
    }
}