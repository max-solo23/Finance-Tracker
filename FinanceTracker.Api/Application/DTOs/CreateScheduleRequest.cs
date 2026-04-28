using System.ComponentModel.DataAnnotations;

namespace FinanceTracker.Api.Application.DTOs;

public class CreateScheduleRequest
{
    [Required]
    public int AccountId { get; set; }
    [Required]
    public string Name { get; set; } = string.Empty;
    [Required]
    public string Description { get; set; } = string.Empty;
    [Required]
    public decimal Amount { get; set; }
    [Required]
    [Range(1, int.MaxValue)]
    public int IntervalMonths { get; set; }
    [Required]
    public DateTime AnchorDate { get; set; }
    public string? Category { get; set; }
}