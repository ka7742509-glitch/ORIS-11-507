using System.Reflection;
using MiniHttpServer.Framework.Attributes;
using MiniHttpServer.Framework.Controllers;

namespace MiniHttpServer.Framework.Routing;

public sealed class RouteResolver
{
    private readonly List<Type> _controllerTypes;

    public RouteResolver(Assembly assembly)
    {
        _controllerTypes = FindControllers(assembly);
    }

    public static RouteResolver Default { get; } =
        new(Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly());

    public RouteInfo? Resolve(string path, string httpMethod)
    {
        string[] segments = path.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length != 2) return null;

        Type? controllerType = _controllerTypes.FirstOrDefault(t =>
            t.Name.Equals(segments[0] + "Controller", StringComparison.OrdinalIgnoreCase));
        if (controllerType is null) return null;

        MethodInfo? method = controllerType
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(m => m.Name.Equals(segments[1], StringComparison.OrdinalIgnoreCase)
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

    private static List<Type> FindControllers(Assembly assembly)
    {
        return assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract
                                  && typeof(BaseController).IsAssignableFrom(t)
                                  && t.GetCustomAttribute<ControllerAttribute>() is not null)
            .ToList();
    }
}