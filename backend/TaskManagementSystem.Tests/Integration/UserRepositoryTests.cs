using Microsoft.Extensions.Configuration;
using TaskManagementSystem.Domain.Entities;
using TaskManagementSystem.Infrastructure.Data;
using TaskManagementSystem.Infrastructure.Repositories;

namespace TaskManagementSystem.Tests.Integration;

public class UserRepositoryTests : IDisposable
{
    private readonly DbConnectionFactory _connectionFactory;
    private readonly UserRepository _repository;

    public UserRepositoryTests()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Port=5433;Database=taskmanagement;Username=postgres;Password=changeme"
            })
            .Build();

        _connectionFactory = new DbConnectionFactory(configuration);
        _repository = new UserRepository(_connectionFactory);
    }

    [Fact]
    public async System.Threading.Tasks.Task CreateAsync_ValidUser_PersistsToDatabase()
    {
        var user = new User(
            Guid.NewGuid(),
            "Integration Test User",
            $"test-{Guid.NewGuid()}@example.com",
            "hash",
            "salt",
            DateTime.UtcNow
        );

        await _repository.CreateAsync(user);

        var retrieved = await _repository.FindByEmailAsync(user.Email);

        Assert.NotNull(retrieved);
        Assert.Equal(user.Id, retrieved.Id);
        Assert.Equal(user.Email, retrieved.Email);
        Assert.Equal(user.Name, retrieved.Name);
    }

    [Fact]
    public async System.Threading.Tasks.Task FindByEmailAsync_ExistingUser_ReturnsUser()
    {
        var result = await _repository.FindByEmailAsync("demo@example.com");

        Assert.NotNull(result);
        Assert.Equal("demo@example.com", result.Email);
        Assert.Equal("Demo User", result.Name);
    }

    [Fact]
    public async System.Threading.Tasks.Task FindByEmailAsync_NonExistentUser_ReturnsNull()
    {
        var result = await _repository.FindByEmailAsync("nonexistent@example.com");

        Assert.Null(result);
    }

    [Fact]
    public async System.Threading.Tasks.Task FindByIdAsync_ExistingUser_ReturnsUser()
    {
        var demoUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        var result = await _repository.FindByIdAsync(demoUserId);

        Assert.NotNull(result);
        Assert.Equal(demoUserId, result.Id);
        Assert.Equal("demo@example.com", result.Email);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
