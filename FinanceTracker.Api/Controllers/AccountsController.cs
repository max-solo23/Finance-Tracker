using System.Threading.Tasks;
using FinanceTracker.Api.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountsController : ControllerBase
{
    private readonly ILogger<AccountsController> _logger;
    private readonly FinanceTrackerContext _context;

    public AccountsController(ILogger<AccountsController> logger, FinanceTrackerContext context)
    {
        _logger = logger;
        _context = context;
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
        
        var account = await _context.Accounts
            .Include(a => a.Transactions)
            .FirstOrDefaultAsync(account => account.Id == id);

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

        var account = new Account(request.Name!);
        
        _context.Accounts.Add(account);

        await _context.SaveChangesAsync();

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

        var account = await _context.Accounts.FindAsync(id);

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

        await _context.SaveChangesAsync();

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

        var account = await _context.Accounts.FindAsync(id);

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

        await _context.SaveChangesAsync();

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

        var account = await _context.Accounts.FindAsync(id);

        if (account == null)
        {
            _logger.LogWarning("Account id={Id} not found for deletion.", id);
            return StatusCode(404, new ErrorResponse
            {
                Message = "Account not found.",
                StatusCode = 404
            });
        }

        _context.Accounts.Remove(account);

        await _context.SaveChangesAsync();

        _logger.LogInformation("Account id={Id} deleted.", id);

        return NoContent();
    }
}