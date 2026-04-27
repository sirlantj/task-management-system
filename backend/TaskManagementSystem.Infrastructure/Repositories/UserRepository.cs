using Npgsql;
using TaskManagementSystem.Application.Interfaces;
using TaskManagementSystem.Domain.Entities;
using TaskManagementSystem.Infrastructure.Data;

namespace TaskManagementSystem.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly DbConnectionFactory _connectionFactory;

    public UserRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT id, name, email, password_hash, password_salt, created_at
            FROM users
            WHERE email = @Email";

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Email", email);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return MapToUser(reader);
    }

    public async Task<User?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT id, name, email, password_hash, password_salt, created_at
            FROM users
            WHERE id = @Id";

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", id);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return MapToUser(reader);
    }

    public async Task CreateAsync(User user, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT INTO users (id, name, email, password_hash, password_salt, created_at)
            VALUES (@Id, @Name, @Email, @PasswordHash, @PasswordSalt, @CreatedAt)";

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", user.Id);
        command.Parameters.AddWithValue("@Name", user.Name);
        command.Parameters.AddWithValue("@Email", user.Email);
        command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
        command.Parameters.AddWithValue("@PasswordSalt", user.PasswordSalt);
        command.Parameters.AddWithValue("@CreatedAt", user.CreatedAt);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static User MapToUser(NpgsqlDataReader reader)
    {
        return new User(
            id: reader.GetGuid(0),
            name: reader.GetString(1),
            email: reader.GetString(2),
            passwordHash: reader.GetString(3),
            passwordSalt: reader.GetString(4),
            createdAt: reader.GetDateTime(5)
        );
    }
}
