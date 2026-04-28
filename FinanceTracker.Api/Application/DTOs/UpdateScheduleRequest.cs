using System.ComponentModel.DataAnnotations;

namespace FinanceTracker.Api.Application.DTOs;

public class UpdateScheduleRequest
{
    public int? AccountId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? Amount { get; set; }
    [Range(1, int.MaxValue)]
    public int? IntervalMonths { get; set; }
    public DateTime? AnchorDate { get; set; }
    public string? Category { get; set; }
}