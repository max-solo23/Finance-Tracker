using System.Threading.Tasks;
using FinanceTracker.Api.Application.DTOs;
using FinanceTracker.Api.Domain;
using FinanceTracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountsController : ControllerBase
{
    private readonly ILogger<AccountsController> _logger;
    private readonly IAccountRepository _context;
    private readonly FinanceTrackerContext _financeTrackerContext;

    public AccountsController(ILogger<AccountsController> logger, IAccountRepository context, FinanceTrackerContext financeTrackerContext)
    {
        _logger = logger;
        _context = context;
        _financeTrackerContext = financeTrackerContext;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAccount(int id)
    {
        if (id <= 0)
        {
            _logger.LogWarning("Invalid account id={Id} - must be positive", id);
            return StatusCode(400,
                new ErrorResponse
                {
                    Message = "ID must be a positive number.",
                    StatusCode = 400
                }
            );
        }
        
        var account = await _context.GetById(id);

        if (account == null)
        {
            _logger.LogWarning("Account id={Id} not found", id);
            return StatusCode(404,
                new ErrorResponse
                {
                    Message = "Account not found.",
                    StatusCode = 404
                }
            );
        }
        
        _logger.LogInformation("Account id={Id} found", id);
        return Ok(account);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAccount([FromBody] CreateAccountRequest request)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid account creation request: {Errors}", ModelState);
            return StatusCode(422,
                new ErrorResponse
                {
                    Message = "Invalid account creation request.",
                    StatusCode = 422,
                    Errors = ModelState
                        .Where(kvp => kvp.Value?.Errors.Count > 0)
                        .ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToList() ?? new List<string>()
                        )
                }
            );
        }

        var account = await _context.Create(request.Name!);

        _logger.LogInformation("Account created Id={Id}, Name={Name}", account.Id, account.Name);

        return CreatedAtAction(
            nameof(GetAccount),
            new { id = account.Id },
            account
        );
    }

    [HttpPost("{id}/transactions")]
    public async Task<IActionResult> CreateTransaction(int id, [FromBody] CreateTransactionRequest request)
    {
        if (request.Amount == 0)
        {
            _logger.LogWarning("Transaction can't be 0.");
            return StatusCode(422, 
                new ErrorResponse
                {
                    Message = "Transaction can't be 0, select positive number for income or negative for expense.",
                    StatusCode = 422
                }
            );
        }    

        var account = await _context.GetById(id);

        if (account == null)
        {
            return StatusCode(404, 
                new ErrorResponse
                {
                    Message = "Account not found.",
                    StatusCode = 404
                }
            );
        }

        var transaction = new Transaction(request.Amount, request.Description, DateTime.UtcNow);

        account.AddTransaction(transaction);

        await _financeTrackerContext.SaveChangesAsync();

        _logger.LogInformation("Transaction created for account {Id}", id);

        return CreatedAtAction(
            nameof(GetAccount),
            new { id = account.Id },
            transaction
        );
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAccount(int id, [FromBody] UpdateAccountRequest request)
    {
        if (id <= 0)
        {
            return StatusCode(400, new ErrorResponse
            {
                Message = "ID must be a positive number.",
                StatusCode = 400
            });
        }

        if (!ModelState.IsValid)
        {
            return StatusCode(422, new ErrorResponse
            {
                Message = "Invalid account update request.",
                StatusCode = 422,
                Errors = ModelState
                    .Where(kvp => kvp.Value?.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToList() ?? new List<string>()
                    )
            });
        }

        var account = await _context.Update(id, request.Name!);

        if (account == null)
        {
            _logger.LogWarning("Account id={Id} not found for update", id);
            return StatusCode(404, new ErrorResponse
            {
                Message = "Account not found.",
                StatusCode = 404
            });
        }

        account.Name = request.Name!;

        await _financeTrackerContext.SaveChangesAsync();

        _logger.LogInformation("Account id={Id} updated to name Name={Name}", id, account.Name);

        return Ok(account);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAccount(int id)
    {
        if (id <= 0)
        {
            return StatusCode(400, new ErrorResponse
            {
                Message = "ID must be a positive number.",
                StatusCode = 400
            });
        }

        var account = await _financeTrackerContext.Accounts.FindAsync(id);

        if (account == null)
        {
            _logger.LogWarning("Account id={Id} not found for deletion.", id);
            return StatusCode(404, new ErrorResponse
            {
                Message = "Account not found.",
                StatusCode = 404
            });
        }

        await _context.Delete(id);

        await _financeTrackerContext.SaveChangesAsync();

        _logger.LogInformation("Account id={Id} deleted.", id);

        return NoContent();
    }

    [HttpPost("bulk-validate")]
    public async Task<IActionResult> BulkValidateAccount([FromBody] BulkValidateRequest request)
    {
        if (!ModelState.IsValid)
        {
            return StatusCode(422, new ErrorResponse
            {
                Message = "Invalid request.",
                StatusCode = 422
            });
        }

        if (request.AccountIds.Any(id => id <= 0))
        {
            return BadRequest(new ErrorResponse
            {
                Message = "All IDs must be positive.",
                StatusCode = 400
            });
        }

        var existingIds = await _context.ExistsByIds(request.AccountIds.ToList());

        var missingIds = request.AccountIds.Except(existingIds).ToList();

        return Ok(new { existing = existingIds, missing = missingIds });
    }

    [HttpPost("transfer")]
    public async Task<IActionResult> Transfer([FromBody] TransferRequest request)
    {
        var existingTransfer = await _financeTrackerContext.Transfers
            .FirstOrDefaultAsync(t => t.IdempotencyKey == request.IdempotencyKey);

        if (existingTransfer != null)
        {
            return Ok(new { message = "Transfer already processed", transferId = existingTransfer.Id });
        }

        var fromAccount = await _context.GetById(request.FromAccountId);
        var toAccount = await _context.GetById(request.ToAccountId);

        if (fromAccount == null)
        {
            return NotFound(new ErrorResponse
            {
                Message = "From account not found",
                StatusCode = 404,
                Errors = new Dictionary<string, List<string>>
                {
                    ["FromAccountId"] = new List<string> { $"Account {request.FromAccountId} does not exist" }
                }
            });
        }

        if (toAccount == null)
        {
            return NotFound(new ErrorResponse
            {
                Message = "To account not found",
                StatusCode = 404,
                Errors = new Dictionary<string, List<string>>
                {
                    ["ToAccountId"] = new List<string> { $"Account {request.ToAccountId} does not exist"}
                }
            });
        }

        var transfer = new Transfer
        {
            FromAccountId = request.FromAccountId,
            ToAccountId = request.ToAccountId,
            Amount = request.Amount,
            Description = request.Description,
            ProcessAt = DateTime.UtcNow,
            IdempotencyKey = request.IdempotencyKey
        };

        _financeTrackerContext.Transfers.Add(transfer);

        try
        {
            await _financeTrackerContext.SaveChangesAsync();
            return Ok(new { message = "Transfer successful", transferId = transfer.Id});
        }
        catch (DbUpdateException)
        {
            existingTransfer = await _financeTrackerContext.Transfers
                .FirstOrDefaultAsync(t => t.IdempotencyKey == request.IdempotencyKey);

            return Ok(new { message = "Transfer already processed", transferId = existingTransfer!.Id });
        }
    }
}