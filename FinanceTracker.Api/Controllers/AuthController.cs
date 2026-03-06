using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FinanceTracker.Api.Application.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IConfiguration configuration, ILogger<AuthController> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        if ( request.Email != "admin@test.com")
        {
            _logger.LogWarning("Login failed for {Email} - user not found", request.Email);
            return Unauthorized(new ErrorResponse
            {
                Message = "Invalid credentials",
                StatusCode = 401
            });            
        }

        if ( request.Password != "password123")
        {
            _logger.LogWarning("Login failed for {Email} - wrong password", request.Email);
            return Unauthorized(new ErrorResponse
            {
                Message = "Invalid credentials",
                StatusCode = 401
            });    
        }

        var jwtSettings = _configuration.GetSection("JwtSettings");

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
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
}