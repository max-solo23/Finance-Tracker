namespace FinanceTracker.Api.Application.DTOs;

public class AccountSummaryDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public decimal Balance { get; set; }
}