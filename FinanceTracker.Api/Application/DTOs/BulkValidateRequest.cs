using System.ComponentModel.DataAnnotations;

namespace FinanceTracker.Api.Application.DTOs;

public class BulkValidateRequest
{
    [Required(ErrorMessage = "AccountIds is required.")]
    [Length(1, 100, ErrorMessage = "At least one AccountId must be provided. Maximum 100 AccountIds allowed.")]
    public int[] AccountIds { get; set; } = [];
}