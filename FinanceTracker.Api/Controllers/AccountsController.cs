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
            return BadRequest("ID must be a positive number.");
        }
        
        var account = await _context.Accounts
            .Include(a => a.Transactions)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (account == null)
        {
            _logger.LogWarning("Account id={Id} not found", id);
            return NotFound();
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
            return BadRequest(ModelState);
        }

        var account = new Account(request.Name);
        
        _context.Accounts.Add(account);

        await _context.SaveChangesAsync();

        _logger.LogInformation("Account created Id={Id}, Name={Name}", account.Id, account.Name);

        return CreatedAtAction(
            nameof(GetAccount),
            new { id = account.Id },
            account
        );
    }
}