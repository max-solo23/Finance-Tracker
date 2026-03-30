using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FinanceTracker.Api.Application.DTOs;
using FinanceTracker.Api.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;
    private readonly IUserRepository _userRepository;

    public AuthController(
        IConfiguration configuration, ILogger<AuthController> logger, IUserRepository userRepository)
    {
        _configuration = configuration;
        _logger = logger;
        _userRepository = userRepository;
    }

    [EnableRateLimiting("login")]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid User login request.");
            return StatusCode(422,
                new ErrorResponse
                {
                   Message = "Invalid User login request.",
                   StatusCode = 422,
                   Errors = ModelState
                       .Where(kvp => kvp.Value?.Errors.Count > 0)
                       .ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToList() ?? new List<string>()
                       )
                });
        }
        var user = await _userRepository.GetByEmail(request.Email);

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

    [EnableRateLimiting("register")]
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

        var emailExists = await _userRepository.ExistsByEmail(request.Email);

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

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        await _userRepository.Create(request.Email, passwordHash);

        _logger.LogInformation("User {Email} successfully created.", request.Email);

        return Created();
    }
}