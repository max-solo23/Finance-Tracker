using System.ComponentModel.DataAnnotations;

namespace FinanceTracker.Api.DTOs;

public class TransferRequest
{
    [Required]
    public int FromAccountId { get; set; }

    [Required]
    public int ToAccountId { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be positive")]
    public decimal Amount { get; set; }

    [Required]
    [MaxLength(200)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string IdempotencyKey { get; set; } = string.Empty;
}