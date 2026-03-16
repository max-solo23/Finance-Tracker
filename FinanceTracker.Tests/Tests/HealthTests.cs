using System.Net;
using FinanceTracker.Tests.Factories;

namespace FinanceTracker.Tests.Tests;

public class HealthTests : IClassFixture<FinanceTrackerFactory>
{
    private readonly HttpClient _client;

    public HealthTests(FinanceTrackerFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task HealthEndpoint_ReturnsOk()
    {
        var response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}