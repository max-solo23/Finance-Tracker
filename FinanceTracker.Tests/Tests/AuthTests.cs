using System.Net;
using System.Text;
using System.Text.Json;
using FinanceTracker.Tests.Factories;

namespace FinanceTracker.Tests.Tests;

public class AuthTests : IClassFixture<FinanceTrackerFactory>
{
    private readonly HttpClient _client;

    public AuthTests(FinanceTrackerFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_ValidCredentials_Returns200()
    {
        var request = new { Email = "test@example.com", Password = "Test123!" };
        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json"
        );

        var registerResponse = await _client.PostAsync("/api/auth/register", content);
        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);

        var response = await _client.PostAsync("/api/auth/login", content);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Login_InvalidPassword_Returns401()
    {
        var request = new { Email = "test@example.com", Password = "wrongpassword" };
        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json"
        );

        var response = await _client.PostAsync("/api/auth/login", content);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}