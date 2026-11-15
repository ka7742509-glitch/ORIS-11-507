using System.Reflection;
using MiniHttpServer.Framework.Attributes;
using MiniHttpServer.Framework.Controllers;

namespace MiniHttpServer.Framework.Routing;

public static class RouteResolver
{
    private static readonly Lazy<List<Type>> ControllerTypes = new(FindControllers);

    public static RouteInfo? Resolve(string path, string httpMethod)
    {
        string[] segments = path.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (segments.Length != 2)
            return null;

        string controllerName = segments[0];
        string methodName = segments[1];

        Type? controllerType = ControllerTypes.Value.FirstOrDefault(type =>
            type.Name.Equals(controllerName + "Controller", StringComparison.OrdinalIgnoreCase));

        if (controllerType is null)
            return null;

        MethodInfo? method = controllerType
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(m => m.Name.Equals(methodName, StringComparison.OrdinalIgnoreCase)
                                 && HasHttpAttribute(m, httpMethod));

        return method is null ? null : new RouteInfo(controllerType, method);
    }

    private static bool HasHttpAttribute(MethodInfo method, string httpMethod)
    {
        return httpMethod.ToUpperInvariant() switch
        {
            "GET" => method.GetCustomAttribute<GetAttribute>() is not null,
            "POST" => method.GetCustomAttribute<PostAttribute>() is not null,
            _ => false
        };
    }

    private static List<Type> FindControllers()
    {
        return Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(type => type.IsClass
                           && !type.IsAbstract
                           && typeof(BaseController).IsAssignableFrom(type)
                           && type.GetCustomAttribute<ControllerAttribute>() is not null)
            .ToList();
    }
}