using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using TaskManagementSystem.Application.DTOs;

namespace TaskManagementSystem.Tests.Api;

public class TasksControllerTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;

    public TasksControllerTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetTasks_WithoutAuthentication_Returns401()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/tasks");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task CreateTask_AuthenticatedUser_Returns201WithTaskBody()
    {
        var client = _factory.CreateClient();

        // Login as the seeded demo user to obtain a valid token
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest("demo@example.com", "Demo@123"));
        loginResponse.EnsureSuccessStatusCode();

        var auth = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(auth);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth.Token);

        var response = await client.PostAsJsonAsync("/api/tasks",
            new CreateTaskRequest("API test task", "Created in API test", null));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var task = await response.Content.ReadFromJsonAsync<TaskResponse>();
        Assert.NotNull(task);
        Assert.Equal("API test task", task.Title);
        Assert.Equal("Pending", task.Status);
        Assert.Equal(auth.UserId, task.UserId);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetTaskById_AnotherUsersTask_Returns404()
    {
        var client = _factory.CreateClient();

        // Login as demo user (user A) and create a task
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest("demo@example.com", "Demo@123"));
        var authA = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(authA);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", authA.Token);

        var createResponse = await client.PostAsJsonAsync("/api/tasks",
            new CreateTaskRequest("User A private task", null, null));
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var taskA = await createResponse.Content.ReadFromJsonAsync<TaskResponse>();
        Assert.NotNull(taskA);

        // Register a second user (user B) with a unique email to avoid conflicts
        var uniqueEmail = $"userb-{Guid.NewGuid()}@example.com";
        var registerResponse = await client.PostAsJsonAsync("/api/auth/register",
            new RegisterUserRequest("User B", uniqueEmail, "Password@123"));
        registerResponse.EnsureSuccessStatusCode();

        var authB = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(authB);

        // Attempt to access user A's task using user B's token — must return 404
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", authB.Token);

        var getResponse = await client.GetAsync($"/api/tasks/{taskA.Id}");

        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }
}
