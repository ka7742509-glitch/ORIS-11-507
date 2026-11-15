using System.Collections.Specialized;
using System.Reflection;

namespace MiniHttpServer.Framework.Routing;

public static class ParameterBinder
{
    public static object?[] Bind(MethodInfo method, NameValueCollection values)
    {
        ParameterInfo[] parameters = method.GetParameters();
        object?[] arguments = new object?[parameters.Length];

        for (int i = 0; i < parameters.Length; i++)
        {
            ParameterInfo parameter = parameters[i];
            string? raw = values[parameter.Name];

            if (string.IsNullOrEmpty(raw))
            {
                if (parameter.HasDefaultValue)
                {
                    arguments[i] = parameter.DefaultValue;
                    continue;
                }

                throw new FormatException($"Отсутствует обязательный параметр '{parameter.Name}'.");
            }

            arguments[i] = ConvertValue(raw, parameter.ParameterType, parameter.Name);
        }

        return arguments;
    }

    private static object ConvertValue(string raw, Type targetType, string parameterName)
    {
        Type type = Nullable.GetUnderlyingType(targetType) ?? targetType;

        try
        {
            if (type == typeof(string))
                return raw;

            if (type.IsEnum)
                return Enum.Parse(type, raw, ignoreCase: true);

            if (type == typeof(Guid))
                return Guid.Parse(raw);

            return Convert.ChangeType(raw, type, System.Globalization.CultureInfo.InvariantCulture);
        }
        catch (Exception ex) when (ex is FormatException or InvalidCastException or OverflowException or ArgumentException)
        {
            throw new FormatException($"Параметр '{parameterName}' имеет некорректное значение '{raw}'.");
        }
    }
}