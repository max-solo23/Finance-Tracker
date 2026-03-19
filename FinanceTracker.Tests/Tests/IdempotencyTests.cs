using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FinanceTracker.Tests.Factories;

namespace FinanceTracker.Tests.Tests;

public class IdempotencyTests : IClassFixture<FinanceTrackerFactory>
{
    private readonly HttpClient _client;

    public IdempotencyTests(FinanceTrackerFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task AuthenticateAsync()
    {
        var user = new { Email = "idempotency@example.com", Password = "Test123!" };
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

        var transactionRequest = new StringContent(
            JsonSerializer.Serialize(new { Amount = balance, Description = "Initial deposit"}),
            Encoding.UTF8,
            "application/json"
        );

        await _client.PostAsync($"/api/accounts/{accountId}/transactions", transactionRequest);

        return accountId;
    }

    [Fact]
    public async Task Transfer_ReturnsOk_WhenDuplicateKey()
    {
        await AuthenticateAsync();

        var fromId = await CreateAccountAsync(500);
        var toId = await CreateAccountAsync(200);

        var idempotencyKey = Guid.NewGuid().ToString();

        var request = new
        {
            FromAccountId = fromId,
            ToAccountId = toId,
            Amount = 50m,
            Description = "Test idempotency",
            IdempotencyKey = idempotencyKey
        };

        var contentResponse1 = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json"
        );

        var response1 = await _client.PostAsync("/api/accounts/transfer", contentResponse1);

        var contentResponse2 = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json"
        );

        var response2 = await _client.PostAsync("/api/accounts/transfer", contentResponse2);

        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
    }

    [Fact]
    public async Task Transfer_ReturnsSameTransferId_WhenDuplicateKey()
    {
        await AuthenticateAsync();

        var fromId = await CreateAccountAsync(500);
        var toId = await CreateAccountAsync(500);

        var idempotencyKey = Guid.NewGuid().ToString();

        var request = new
        {
            FromAccountId = fromId,
            ToAccountId = toId,
            Amount = 50,
            Description = "Test Duplicate Key",
            IdempotencyKey = idempotencyKey
        };

        var content1 = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json"
        );

        var response1 = await _client.PostAsync("/api/accounts/transfer", content1);
        var body1 = await response1.Content.ReadAsStringAsync();
        var json1 = JsonDocument.Parse(body1);
        var transferId1 = json1.RootElement.GetProperty("transferId").GetInt32();

        var content2 = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json"
        );

        var response2 = await _client.PostAsync("/api/accounts/transfer", content2);
        var body2 = await response2.Content.ReadAsStringAsync();
        var json2 = JsonDocument.Parse(body2);
        var transferId2 = json2.RootElement.GetProperty("transferId").GetInt32();

        Assert.Equal(transferId1, transferId2);
    }
}