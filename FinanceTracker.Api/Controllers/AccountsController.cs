using FinanceTracker.Api.Application;
using FinanceTracker.Api.Application.DTOs;
using FinanceTracker.Api.Domain;
using Microsoft.AspNetCore.Mvc;

namespace FinanceTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountsController : ControllerBase
{
    private readonly ILogger<AccountsController> _logger;
    private readonly IAccountService _accountService;
    private readonly ITransactionRepository _transactionRepository;
    private readonly ITransferRepository _transferRepository;

    public AccountsController(
        ILogger<AccountsController> logger, 
        IAccountService accountService, 
        ITransactionRepository transactionRepository, 
        ITransferRepository transferRepository
    )
    {
        _logger = logger;
        _accountService = accountService;
        _transactionRepository = transactionRepository;
        _transferRepository = transferRepository;
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
        
        var account = await _accountService.GetById(id);

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

        var account = await _accountService.Create(request.Name!);

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

        var account = await _accountService.GetById(id);

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

        var transaction = await _transactionRepository.Create(id, request.Amount, request.Description);

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

        var account = await _accountService.Update(id, request.Name!);

        if (account == null)
        {
            _logger.LogWarning("Account id={Id} not found for update", id);
            return StatusCode(404, new ErrorResponse
            {
                Message = "Account not found.",
                StatusCode = 404
            });
        }

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

        var accountDeleted = await _accountService.Delete(id);

        if (!accountDeleted)
        {
            _logger.LogWarning("Account id={Id} not found for deletion.", id);
            return StatusCode(404, new ErrorResponse
            {
                Message = "Account not found.",
                StatusCode = 404
            });
        }

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

        var existingIds = await _accountService.ExistsByIds(request.AccountIds.ToList());

        var missingIds = request.AccountIds.Except(existingIds).ToList();

        return Ok(new { existing = existingIds, missing = missingIds });
    }

    [HttpPost("transfer")]
    public async Task<IActionResult> Transfer([FromBody] TransferRequest request)
    {
        var existingTransfer = await _transferRepository.GetByIdempotencyKey(request.IdempotencyKey);

        if (existingTransfer != null)
        {
            return Ok(new { message = "Transfer already processed", transferId = existingTransfer.Id });
        }

        var fromAccount = await _accountService.GetById(request.FromAccountId);
        var toAccount = await _accountService.GetById(request.ToAccountId);

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

        var transfer = await _transferRepository.Create(
            request.FromAccountId,
            request.ToAccountId,
            request.Amount,
            request.Description,
            request.IdempotencyKey
        );
        
        return Ok(new { message = "Transfer successful", transferId = transfer.Id});
    }
}