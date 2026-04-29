using FinanceTracker.Api.Domain;
using FinanceTracker.Models;

namespace FinanceTracker.Api.Infrastructure;

public class RecurringScheduleRepository : IRecurringScheduleRepository
{
    private readonly FinanceTrackerContext _context;
    public RecurringScheduleRepository(FinanceTrackerContext financeTrackerContext)
    {
        _context = financeTrackerContext;
    }

    public Task<RecurringSchedule> Create(RecurringSchedule schedule)
    {
        throw new NotImplementedException();
    }

    public Task Delete(RecurringSchedule schedule)
    {
        throw new NotImplementedException();
    }

    public Task<List<RecurringSchedule>> GetAll(int userId)
    {
        throw new NotImplementedException();
    }

    public Task<RecurringSchedule?> GetById(int id, int userId)
    {
        throw new NotImplementedException();
    }

    public Task Update(RecurringSchedule schedule)
    {
        throw new NotImplementedException();
    }
}