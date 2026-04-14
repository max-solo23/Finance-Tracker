
using FinanceTracker.Api.Application.DTOs;
using FinanceTracker.Api.Domain;

namespace FinanceTracker.Api.Application.Services;

public class AccountService : IAccountService
{
    private readonly IAccountRepository _repository;

    public AccountService(IAccountRepository accountRepository)
    {
        _repository = accountRepository;
    }
    
    public Task<Account> Create(string name, int userId)
    {
        var account = _repository.Create(name, userId);

        return account;
    }

    public Task<bool> Delete(int id, int userId)
    {
        var result = _repository.Delete(id, userId);

        return result;
    }

    public async Task<List<int>> ExistsByIds(List<int> ids, int userId)
    {
        var existingIds = await _repository.ExistsByIds(ids, userId);

        return existingIds;
    }

    public async Task<PagedResponse<AccountSummaryDto>> GetAll(int userId, int page, int pageSize)
    {
        pageSize = Math.Clamp(pageSize, 1, 50);

        var totalCount = await _repository.GetCount(userId);

        var accounts = await _repository.GetPaged(userId, page, pageSize);

        return new PagedResponse<AccountSummaryDto>(accounts, totalCount, page, pageSize);
    }

    public Task<Account?> GetById(int id, int userId)
    {
        var account = _repository.GetById(id, userId);

        return account;
    }

    public Task<Account?> Update(int id, string name, int userId)
    {
        var account = _repository.Update(id, name, userId);

        return account;
    }


}