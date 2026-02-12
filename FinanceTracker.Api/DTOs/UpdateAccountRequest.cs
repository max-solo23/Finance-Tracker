using System.ComponentModel.DataAnnotations;

namespace FinanceTracker.Api.DTOs;

public class UpdateAccountRequest
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string? Name { get; set; }
}