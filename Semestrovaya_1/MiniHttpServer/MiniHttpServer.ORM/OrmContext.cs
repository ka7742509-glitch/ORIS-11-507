using System.Globalization;
using System.Reflection;
using Npgsql;

namespace MiniHttpServer.ORM;

public sealed class OrmContext : IOrmContext
{
    private readonly string _connectionString;

    public OrmContext(string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        _connectionString = connectionString;
    }

    public async Task<IReadOnlyList<T>> GetAllAsync<T>(string tableName, CancellationToken cancellationToken = default)
        where T : class, new()
    {
        string sql = SqlBuilder.BuildSelect(tableName);
        var result = new List<T>();

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
            result.Add(EntityMapper.Map<T>(await ReadRowAsync(reader, cancellationToken)));

        return result;
    }

    public async Task<T?> GetByIdAsync<T>(string tableName, int id, CancellationToken cancellationToken = default)
        where T : class, new()
    {
        string sql = SqlBuilder.BuildSelectById(tableName);

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", id);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return EntityMapper.Map<T>(await ReadRowAsync(reader, cancellationToken));
    }

    public async Task<int> InsertAsync<T>(string tableName, T entity, CancellationToken cancellationToken = default)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(entity);

        var properties = EntityMapper.GetInsertProperties(typeof(T));
        if (properties.Length == 0)
            throw new InvalidOperationException($"Тип {typeof(T).Name} не содержит свойств для вставки.");

        string sql = SqlBuilder.BuildInsert(tableName, properties.Select(p => p.Name).ToArray());

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);

        foreach (var property in properties)
            command.Parameters.AddWithValue("@" + property.Name, EntityMapper.ToParameterValue(property.GetValue(entity)));

        return await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<int> UpdateAsync<T>(string tableName, int id, T entity, CancellationToken cancellationToken = default)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(entity);

        var properties = EntityMapper.GetUpdateProperties(typeof(T));
        if (properties.Length == 0)
            throw new InvalidOperationException($"Тип {typeof(T).Name} не содержит свойств для обновления.");

        string sql = SqlBuilder.BuildUpdate(tableName, properties.Select(p => p.Name).ToArray());

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("@Id", id);

        foreach (var property in properties)
            command.Parameters.AddWithValue("@" + property.Name, EntityMapper.ToParameterValue(property.GetValue(entity)));

        return await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<int> DeleteAsync(string tableName, int id, CancellationToken cancellationToken = default)
    {
        string sql = SqlBuilder.BuildDelete(tableName);

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", id);

        return await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<Dictionary<string, object?>> ReadRowAsync(NpgsqlDataReader reader, CancellationToken cancellationToken)
    {
        var row = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i < reader.FieldCount; i++)
            row[reader.GetName(i)] = await reader.IsDBNullAsync(i, cancellationToken) ? null : reader.GetValue(i);

        return row;
    }
}