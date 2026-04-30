using FinanceTracker.Api.Domain;
using FinanceTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Api.Infrastructure;

public class RecurringScheduleRepository : IRecurringScheduleRepository
{
    private readonly FinanceTrackerContext _context;
    public RecurringScheduleRepository(FinanceTrackerContext financeTrackerContext)
    {
        _context = financeTrackerContext;
    }

    public async Task<RecurringSchedule> Create(RecurringSchedule schedule)
    {
        _context.RecurringSchedules.Add(schedule);

        await _context.SaveChangesAsync();

        return schedule;
    }

    public async Task Delete(RecurringSchedule schedule)
    {
        _context.RecurringSchedules.Remove(schedule);

        await _context.SaveChangesAsync();
    }

    public async Task<List<RecurringSchedule>> GetAll(int userId)
    {
        return await _context.RecurringSchedules
            .AsNoTracking()
            .Where(recurringSchedule => recurringSchedule.UserId == userId)
            .ToListAsync();
    }

    public async Task<RecurringSchedule?> GetById(int id, int userId)
    {        
        return await _context.RecurringSchedules
            .AsNoTracking()
            .Where(recurringSchedule => recurringSchedule.Id == id && recurringSchedule.UserId == userId)
            .FirstOrDefaultAsync();        
    }

    public async Task Update(RecurringSchedule schedule)
    {
        _context.RecurringSchedules.Update(schedule);
        
        await _context.SaveChangesAsync();
    }
}