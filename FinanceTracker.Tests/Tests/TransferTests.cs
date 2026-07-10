using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FinanceTracker.Tests.Factories;

namespace FinanceTracker.Tests.Tests;

public class TransferTests : IClassFixture<FinanceTrackerFactory>
{
    private readonly HttpClient _client;

    public TransferTests(FinanceTrackerFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task AuthenticateAsync()
    {
        var user = new { Email = "transfer@example.com", Password = "Test123!" };
        var content = new StringContent(
            JsonSerializer.Serialize(user),
            Encoding.UTF8,
            "application/json"
        );

        await _client.PostAsync("/api/auth/register", content);
        var loginResponse = await _client.PostAsync("/api/auth/login", content);

        var body = await loginResponse.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(body);
        var token = json.RootElement.GetProperty("token").GetString();

        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);
    }

    private async Task<int> CreateAccountAsync(decimal balance)
    {
        var request = new { Name = "Test Account" };
        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json"
        );

        var response = await _client.PostAsync("/api/accounts", content);
        var body = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(body);
        var accountId = json.RootElement.GetProperty("id").GetInt32();

        var transactionContent = new StringContent(
            JsonSerializer.Serialize(new { Amount = balance, Description = "Initial deposit" }),
            Encoding.UTF8,
            "application/json"
        );

        await _client.PostAsync($"/api/accounts/{accountId}/transactions", transactionContent);

        return accountId;
    }

    [Fact]
    public async Task Transfer_ReturnsOk_WhenValid()
    {
        await AuthenticateAsync();

        var fromId = await CreateAccountAsync(2000);
        var toId = await CreateAccountAsync(2500);

        var request = new
        {
            FromAccountId = fromId,
            ToAccountId = toId,
            Amount = 500,
            Description = "Test transfer",
            IdempotencyKey = Guid.NewGuid().ToString()
        };

        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json"
        );

        var response = await _client.PostAsync("/api/accounts/transfer", content);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Transfer_Returns401_WhenUnauthenticated()
    {
        var request = new
        {
            FromAccountId = 1,
            ToAccountId = 2,
            Amount = 50m,
            Description = "No Auth",
            IdempotencyKey = Guid.NewGuid().ToString()
        };

        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json"
        );

        var response = await _client.PostAsync("/api/accounts/transfer", content);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Transfer_Returns422_WhenInsufficientFunds()
    {
        await AuthenticateAsync();

        var fromId = await CreateAccountAsync(300);
        var toId = await CreateAccountAsync(400);

        var request = new
        {
            FromAccountId = fromId,
            ToAccountId = toId,
            Amount = 320,
            Description = "Too Much",
            IdempotencyKey = Guid.NewGuid().ToString()
        };

        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json"
        );

        var response = await _client.PostAsync("/api/accounts/transfer", content);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task Transfer_Returns422_WhenSelfTransfer()
    {
        await AuthenticateAsync();

        var accountId = await CreateAccountAsync(500m);

        var request = new
        {
            FromAccountId = accountId,
            ToAccountId = accountId,
            Amount = 500m,
            Description = "Self",
            IdempotencyKey = Guid.NewGuid().ToString()
        };

        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json"
        );

        var response = await _client.PostAsync("/api/accounts/transfer", content);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task Transfer_Returns422_WhenAmountIsNegative()
    {
        await AuthenticateAsync();

        var fromAccountId = await CreateAccountAsync(100m);
        var toAccountId = await CreateAccountAsync(100m);

        var request = new
        {
            FromAccountId = fromAccountId,
            ToAccountId = toAccountId,
            Amount = -50m,
            Description = "Negative transfer",
            IdempotencyKey = Guid.NewGuid().ToString()
        };

        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json"
        );

        var response = await _client.PostAsync("/api/accounts/transfer", content);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }
}    
