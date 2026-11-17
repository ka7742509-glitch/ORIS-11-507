using System.Globalization;
using System.Reflection;

namespace MiniHttpServer.ORM;

public static class EntityMapper
{
    public static PropertyInfo[] GetWritableProperties(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        return type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite && p.SetMethod?.IsPublic == true)
            .ToArray();
    }

    public static PropertyInfo[] GetInsertProperties(Type type)
        => GetWritableProperties(type).Where(p => !IsPrimaryKey(p)).ToArray();

    public static PropertyInfo[] GetUpdateProperties(Type type)
        => GetWritableProperties(type).Where(p => !IsPrimaryKey(p)).ToArray();

    public static T Map<T>(IReadOnlyDictionary<string, object?> row) where T : class, new()
    {
        ArgumentNullException.ThrowIfNull(row);

        var entity = new T();

        foreach (PropertyInfo property in GetWritableProperties(typeof(T)))
        {
            if (!TryGetValue(row, property.Name, out object? value))
                continue;

            property.SetValue(entity, ConvertValue(value, property.PropertyType));
        }

        return entity;
    }

    public static object? ConvertValue(object? value, Type targetType)
    {
        ArgumentNullException.ThrowIfNull(targetType);

        Type underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;

        if (value is null || value is DBNull)
        {
            if (underlying.IsValueType && Nullable.GetUnderlyingType(targetType) is null)
                return Activator.CreateInstance(targetType);

            return null;
        }

        if (underlying.IsInstanceOfType(value))
            return value;

        if (underlying.IsEnum)
            return Enum.ToObject(underlying, value);

        return Convert.ChangeType(value, underlying, CultureInfo.InvariantCulture);
    }

    public static object ToParameterValue(object? value) => value ?? DBNull.Value;

    private static bool IsPrimaryKey(PropertyInfo property)
        => property.Name.Equals("Id", StringComparison.OrdinalIgnoreCase);

    private static bool TryGetValue(IReadOnlyDictionary<string, object?> row, string key, out object? value)
    {
        foreach (var pair in row)
        {
            if (pair.Key.Equals(key, StringComparison.OrdinalIgnoreCase))
            {
                value = pair.Value;
                return true;
            }
        }

        value = null;
        return false;
    }
}