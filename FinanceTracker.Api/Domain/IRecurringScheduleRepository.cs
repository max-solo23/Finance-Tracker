using FinanceTracker.Models;

namespace FinanceTracker.Api.Domain;

public interface IRecurringScheduleRepository
{
    Task<List<RecurringSchedule>> GetAll(int userId);
    Task<RecurringSchedule?> GetById(int id, int userId);
    Task<RecurringSchedule> Create(RecurringSchedule schedule);
    Task Update(RecurringSchedule schedule);
    Task Delete(RecurringSchedule schedule);
}