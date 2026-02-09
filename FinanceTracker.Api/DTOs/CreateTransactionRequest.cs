using System.ComponentModel.DataAnnotations;

namespace FinanceTracker.Api.DTOs;

public class CreateTransactionRequest
{
    public decimal Amount { set; get; }
    [Required]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Description must be maximum 100 characters.")]
    public string Description { set; get; }  = string.Empty;
}