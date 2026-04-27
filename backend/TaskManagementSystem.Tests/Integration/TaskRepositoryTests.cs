using Microsoft.Extensions.Configuration;
using TaskManagementSystem.Domain.Entities;
using TaskManagementSystem.Domain.Enums;
using TaskManagementSystem.Infrastructure.Data;
using TaskManagementSystem.Infrastructure.Repositories;

namespace TaskManagementSystem.Tests.Integration;

public class TaskRepositoryTests : IDisposable
{
    private readonly DbConnectionFactory _connectionFactory;
    private readonly TaskRepository _repository;
    private readonly Guid _demoUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public TaskRepositoryTests()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Port=5433;Database=taskmanagement;Username=postgres;Password=changeme"
            })
            .Build();

        _connectionFactory = new DbConnectionFactory(configuration);
        _repository = new TaskRepository(_connectionFactory);
    }

    [Fact]
    public async System.Threading.Tasks.Task CreateAsync_ValidTask_PersistsToDatabase()
    {
        var task = new TaskItem(
            Guid.NewGuid(),
            "Integration test task",
            "Test description",
            TaskStatus.Pending,
            null,
            _demoUserId,
            DateTime.UtcNow,
            null
        );

        await _repository.CreateAsync(task);

        var retrieved = await _repository.GetByIdAsync(task.Id, _demoUserId);

        Assert.NotNull(retrieved);
        Assert.Equal(task.Id, retrieved.Id);
        Assert.Equal(task.Title, retrieved.Title);
        Assert.Equal(task.Status, retrieved.Status);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetAllByUserIdAsync_ReturnsOnlyUserTasks()
    {
        var tasks = await _repository.GetAllByUserIdAsync(_demoUserId);

        Assert.NotEmpty(tasks);
        Assert.All(tasks, t => Assert.Equal(_demoUserId, t.UserId));
    }

    [Fact]
    public async System.Threading.Tasks.Task GetByIdAsync_AnotherUsersTask_ReturnsNull()
    {
        var tasks = await _repository.GetAllByUserIdAsync(_demoUserId);
        var existingTask = tasks.First();
        var differentUserId = Guid.NewGuid();

        var result = await _repository.GetByIdAsync(existingTask.Id, differentUserId);

        Assert.Null(result);
    }

    [Fact]
    public async System.Threading.Tasks.Task UpdateAsync_ValidTask_UpdatesInDatabase()
    {
        var task = new TaskItem(
            Guid.NewGuid(),
            "Update test task",
            null,
            TaskStatus.Pending,
            null,
            _demoUserId,
            DateTime.UtcNow,
            null
        );

        await _repository.CreateAsync(task);

        task.UpdateTitle("Updated title");
        task.TransitionStatus(TaskStatus.InProgress);

        await _repository.UpdateAsync(task);

        var retrieved = await _repository.GetByIdAsync(task.Id, _demoUserId);

        Assert.NotNull(retrieved);
        Assert.Equal("Updated title", retrieved.Title);
        Assert.Equal(TaskStatus.InProgress, retrieved.Status);
    }

    [Fact]
    public async System.Threading.Tasks.Task DeleteAsync_ExistingTask_RemovesFromDatabase()
    {
        var task = new TaskItem(
            Guid.NewGuid(),
            "Delete test task",
            null,
            TaskStatus.Pending,
            null,
            _demoUserId,
            DateTime.UtcNow,
            null
        );

        await _repository.CreateAsync(task);

        await _repository.DeleteAsync(task.Id, _demoUserId);

        var retrieved = await _repository.GetByIdAsync(task.Id, _demoUserId);

        Assert.Null(retrieved);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
