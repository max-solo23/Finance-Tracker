using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace FinanceTracker.Tests.Factories;

public class FinanceTrackerFactory : WebApplicationFactory<Program>
{
    private readonly string _dbPath = $"test_{Guid.NewGuid()}.db";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = $"DataSource={_dbPath}",
                ["JwtSettings:SecretKey"] = "test-secret-key-for-integration-tests-only!",
                ["JwtSettings:Issuer"] = "FinanceTracker",
                ["JwtSettings:Audience"] = "FinanceTrackerUsers",
                ["Cors:AllowedOrigins"] = "http://localhost:3000"
            });
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (File.Exists(_dbPath))
            File.Delete(_dbPath);
    }
}