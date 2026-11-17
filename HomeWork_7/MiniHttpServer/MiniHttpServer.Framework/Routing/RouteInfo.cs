using System.Reflection;

namespace MiniHttpServer.Framework.Routing;

public sealed class RouteInfo
{
    public Type ControllerType { get; }
    public MethodInfo Method { get; }

    public RouteInfo(Type controllerType, MethodInfo method)
    {
        ControllerType = controllerType;
        Method = method;
    }
}