using System.ComponentModel.DataAnnotations;

namespace FinanceTracker.Api.Application.DTOs;

public class CreateTransactionRequest
{
    public decimal Amount { set; get; }
    [Required]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Description must be maximum 100 characters.")]
    public string Description { set; get; }  = string.Empty;
    [StringLength(20, MinimumLength = 1, ErrorMessage = "Category must be maximum 20 characters.")]
    public string? Category { set; get; }
    public DateTime? Date { get; set; }
}