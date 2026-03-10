using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using FinanceTracker.Api.Application.DTOs;
using FinanceTracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;
    private readonly FinanceTrackerContext _context;

    public AuthController(
        IConfiguration configuration, ILogger<AuthController> logger, FinanceTrackerContext context)
    {
        _configuration = configuration;
        _logger = logger;
        _context = context;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Login failed for {Email} - invalid credentials.", request.Email);
            return Unauthorized(new ErrorResponse
            {
                Message = "Invalid credentials",
                StatusCode = 401
            });
        }

        var jwtSettings = _configuration.GetSection("JwtSettings");

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, request.Email)
        };

        var secretKey = jwtSettings["SecretKey"]!;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds
        );

        _logger.LogInformation("User {Email} successfully logged in", request.Email);

        return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token)});
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid User registration request.");
            return StatusCode(422, 
                new ErrorResponse
            {
                Message = "Invalid User registration request.",
                StatusCode = 422,
                Errors = ModelState
                    .Where(kvp => kvp.Value?.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToList() ?? new List<string>()
                    )
            });
        }

        var emailExists = await _context.Users.AnyAsync(user => user.Email == request.Email);

        if (emailExists)
        {
            _logger.LogWarning("Email already used by another user.");
            return StatusCode(409,
                new ErrorResponse
                {
                    Message = "Email already used by another user.",
                    StatusCode = 409
                });
        }

        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new User{
            Email = request.Email, 
            PasswordHash = hashedPassword,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        _logger.LogInformation("User {Email} successfully created.", request.Email);

        return Created();
    }
}