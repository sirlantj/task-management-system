using Npgsql;
using TaskManagementSystem.Application.Interfaces;
using TaskManagementSystem.Domain.Entities;
using TaskManagementSystem.Domain.Enums;
using TaskManagementSystem.Infrastructure.Data;

namespace TaskManagementSystem.Infrastructure.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly DbConnectionFactory _connectionFactory;

    public TaskRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<TaskItem?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT id, title, description, status, due_date, user_id, created_at, updated_at
            FROM tasks
            WHERE id = @Id AND user_id = @UserId";

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", id);
        command.Parameters.AddWithValue("@UserId", userId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return MapToTaskItem(reader);
    }

    public async Task<IEnumerable<TaskItem>> GetAllByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT id, title, description, status, due_date, user_id, created_at, updated_at
            FROM tasks
            WHERE user_id = @UserId
            ORDER BY created_at DESC";

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@UserId", userId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var tasks = new List<TaskItem>();
        while (await reader.ReadAsync(cancellationToken))
        {
            tasks.Add(MapToTaskItem(reader));
        }

        return tasks;
    }

    public async Task CreateAsync(TaskItem task, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT INTO tasks (id, title, description, status, due_date, user_id, created_at, updated_at)
            VALUES (@Id, @Title, @Description, @Status, @DueDate, @UserId, @CreatedAt, @UpdatedAt)";

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", task.Id);
        command.Parameters.AddWithValue("@Title", task.Title);
        command.Parameters.AddWithValue("@Description", (object?)task.Description ?? DBNull.Value);
        command.Parameters.AddWithValue("@Status", task.Status.ToString());
        command.Parameters.AddWithValue("@DueDate", (object?)task.DueDate ?? DBNull.Value);
        command.Parameters.AddWithValue("@UserId", task.UserId);
        command.Parameters.AddWithValue("@CreatedAt", task.CreatedAt);
        command.Parameters.AddWithValue("@UpdatedAt", (object?)task.UpdatedAt ?? DBNull.Value);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task UpdateAsync(TaskItem task, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE tasks
            SET title = @Title,
                description = @Description,
                status = @Status,
                due_date = @DueDate,
                updated_at = @UpdatedAt
            WHERE id = @Id AND user_id = @UserId";

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", task.Id);
        command.Parameters.AddWithValue("@UserId", task.UserId);
        command.Parameters.AddWithValue("@Title", task.Title);
        command.Parameters.AddWithValue("@Description", (object?)task.Description ?? DBNull.Value);
        command.Parameters.AddWithValue("@Status", task.Status.ToString());
        command.Parameters.AddWithValue("@DueDate", (object?)task.DueDate ?? DBNull.Value);
        command.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            DELETE FROM tasks
            WHERE id = @Id AND user_id = @UserId";

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", id);
        command.Parameters.AddWithValue("@UserId", userId);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static TaskItem MapToTaskItem(NpgsqlDataReader reader)
    {
        return TaskItem.Reconstitute(
            id: reader.GetGuid(0),
            title: reader.GetString(1),
            description: reader.IsDBNull(2) ? null : reader.GetString(2),
            status: Enum.Parse<TaskStatus>(reader.GetString(3)),
            dueDate: reader.IsDBNull(4) ? null : reader.GetDateTime(4),
            userId: reader.GetGuid(5),
            createdAt: reader.GetDateTime(6),
            updatedAt: reader.IsDBNull(7) ? null : reader.GetDateTime(7)
        );
    }
}
