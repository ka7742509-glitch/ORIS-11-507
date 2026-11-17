using System.Text.RegularExpressions;

namespace MiniHttpServer.ORM;

public static class SqlBuilder
{
    private static readonly Regex IdentifierRegex = new(@"^[A-Za-z_][A-Za-z0-9_]*$", RegexOptions.Compiled);

    public static string EscapeIdentifier(string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier) || !IdentifierRegex.IsMatch(identifier))
            throw new ArgumentException("Недопустимое имя SQL-идентификатора.", nameof(identifier));

        return $"[{identifier}]";
    }

    public static string BuildSelect(string tableName)
        => $"SELECT * FROM {EscapeIdentifier(tableName)}";

    public static string BuildSelectById(string tableName)
        => $"SELECT * FROM {EscapeIdentifier(tableName)} WHERE [Id] = @Id";

    public static string BuildInsert(string tableName, IReadOnlyList<string> columns)
    {
        string table = EscapeIdentifier(tableName);
        EnsureColumns(columns);

        string names = string.Join(", ", columns.Select(EscapeIdentifier));
        string values = string.Join(", ", columns.Select(c => "@" + c));

        return $"INSERT INTO {table} ({names}) VALUES ({values})";
    }

    public static string BuildUpdate(string tableName, IReadOnlyList<string> columns)
    {
        string table = EscapeIdentifier(tableName);
        EnsureColumns(columns);

        if (columns.Any(c => c.Equals("Id", StringComparison.OrdinalIgnoreCase)))
            throw new ArgumentException("Первичный ключ Id нельзя изменять.", nameof(columns));

        string assignments = string.Join(", ", columns.Select(c => $"{EscapeIdentifier(c)} = @{c}"));

        return $"UPDATE {table} SET {assignments} WHERE [Id] = @Id";
    }

    public static string BuildDelete(string tableName)
        => $"DELETE FROM {EscapeIdentifier(tableName)} WHERE [Id] = @Id";

    private static void EnsureColumns(IReadOnlyList<string> columns)
    {
        if (columns is null || columns.Count == 0)
            throw new ArgumentException("Список столбцов не может быть пустым.", nameof(columns));
    }
}