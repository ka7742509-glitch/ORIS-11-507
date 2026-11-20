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

        if (underlying == typeof(int) && value is long longVal)
            return (int)longVal;

        if (underlying == typeof(int) && value is short shortVal)
            return (int)shortVal;

        if (underlying == typeof(int) && value is byte byteVal)
            return (int)byteVal;

        if (underlying == typeof(long) && value is int intVal)
            return (long)intVal;

        if (underlying == typeof(decimal) && value is double doubleVal)
            return (decimal)doubleVal;

        if (underlying == typeof(decimal) && value is float floatVal)
            return (decimal)floatVal;

        if (underlying == typeof(double) && value is float floatVal2)
            return (double)floatVal2;

        if (underlying == typeof(float) && value is double doubleVal2)
            return (float)doubleVal2;

        if (underlying == typeof(DateTime) && value is DateTime dt)
            return dt;

        if (underlying == typeof(DateTime) && value is string dateStr)
        {
            if (DateTime.TryParse(dateStr, out DateTime parsed))
                return parsed;
        }

        if (underlying.IsEnum)
            return Enum.ToObject(underlying, value);

        try
        {
            return Convert.ChangeType(value, underlying, CultureInfo.InvariantCulture);
        }
        catch (InvalidCastException)
        {
            string strValue = value.ToString() ?? string.Empty;
            return ParseFromString(strValue, underlying);
        }
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

    private static object? ParseFromString(string value, Type targetType)
    {
        if (targetType == typeof(int)) return int.Parse(value, CultureInfo.InvariantCulture);
        if (targetType == typeof(long)) return long.Parse(value, CultureInfo.InvariantCulture);
        if (targetType == typeof(decimal)) return decimal.Parse(value, CultureInfo.InvariantCulture);
        if (targetType == typeof(double)) return double.Parse(value, CultureInfo.InvariantCulture);
        if (targetType == typeof(float)) return float.Parse(value, CultureInfo.InvariantCulture);
        if (targetType == typeof(bool)) return bool.Parse(value);

        if (targetType == typeof(DateTime))
        {
            if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime isoDate))
                return isoDate;
            if (DateTime.TryParse(value, new CultureInfo("ru-RU"), DateTimeStyles.None, out DateTime ruDate))
                return ruDate;
            if (DateTime.TryParse(value, out DateTime defaultDate))
                return defaultDate;
            throw new FormatException($"Не удалось распознать дату: '{value}'");
        }

        if (targetType == typeof(Guid)) return Guid.Parse(value);

        return Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
    }
}