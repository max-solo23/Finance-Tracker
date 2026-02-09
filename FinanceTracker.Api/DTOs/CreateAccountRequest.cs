using System.ComponentModel.DataAnnotations;

namespace FinanceTracker.Api.DTOs;

public class CreateAccountRequest
{
    [Required(ErrorMessage = "Account name is required")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 100 characters")]
    public string? Name { get; set; }
}