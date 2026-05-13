namespace FinanceTracker.Api.Application.DTOs;

public class UpdateTransactionRequest
{
    public decimal? Amount { get; set; }
    public string? Description { get; set; }
    public DateTime? Date { get; set; }
    public string? Category { get; set; }
}