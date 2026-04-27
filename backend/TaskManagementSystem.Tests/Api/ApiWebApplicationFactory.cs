using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace TaskManagementSystem.Tests.Api;

public class ApiWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"] = "test-only-secret-key-for-api-tests-must-be-32chars",
                ["Jwt:Issuer"] = "TaskManagementSystem",
                ["Jwt:Audience"] = "TaskManagementSystem",
                ["Jwt:ExpiresMinutes"] = "60",
                ["ConnectionStrings:DefaultConnection"] =
                    "Host=localhost;Port=5433;Database=taskmanagement;Username=postgres;Password=changeme"
            });
        });
    }
}
