using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using FinanceTracker.Api.Domain;

namespace FinanceTracker.Tests.Factories;

public class FinanceTrackerFactory : WebApplicationFactory<Program>
{
    private readonly string _dbPath = $"test_{Guid.NewGuid()}.db";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("JwtSettings:SecretKey", "test-secret-key-for-integration-test-only!!");
        builder.UseSetting("JwtSettings:Issuer", "FinanceTracker");
        builder.UseSetting("JwtSettings:Audience", "FinanceTrackerUsers");
        builder.UseSetting("Cors:AllowedOrigins", "http://localhost:3000");

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<FinanceTrackerContext>));
            if (descriptor != null) services.Remove(descriptor);

            services.AddDbContext<FinanceTrackerContext>(options => 
                options.UseSqlite($"DataSource={_dbPath}"));
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        try
        {
            if (File.Exists(_dbPath))
                File.Delete(_dbPath);
        }
        catch (IOException)
        {
            
        }
    }
}