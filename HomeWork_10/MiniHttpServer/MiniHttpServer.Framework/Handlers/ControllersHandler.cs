using System.Collections.Specialized;
using System.Net;
using System.Reflection;
using MiniHttpServer.Framework.Authentication;
using MiniHttpServer.Framework.Attributes;
using MiniHttpServer.Framework.Configuration;
using MiniHttpServer.Framework.Controllers;
using MiniHttpServer.Framework.Results;
using MiniHttpServer.Framework.Routing;

namespace MiniHttpServer.Framework.Handlers;

public sealed class ControllersHandler : RequestHandler
{
    private readonly IUserSessionService _sessionService;
    private readonly RouteResolver _routeResolver;

    public ControllersHandler(IUserSessionService sessionService, RouteResolver? routeResolver = null)
    {
        _sessionService = sessionService;
        _routeResolver = routeResolver ?? RouteResolver.Default;
    }

    public override async Task HandleAsync(HttpListenerContext context)
    {
        var request = context.Request;
        string localPath = request.Url!.LocalPath;
        string basePath = ConfigurationManager.Instance.Settings.BasePath;

        if (!localPath.StartsWith(basePath))
        {
            await HandleNextAsync(context);
            return;
        }

        string relativePath = localPath.Substring(basePath.Length);
        RouteInfo? route = _routeResolver.Resolve(relativePath, request.HttpMethod);

        if (route is null)
        {
            await HandleNextAsync(context);
            return;
        }

        if (!IsAuthorized(context, route.ControllerType, route.Method))
        {
            await new JsonResult(new { success = false, message = "Требуется аутентификация" }, 401)
                .ExecuteAsync(context.Response);
            return;
        }

        NameValueCollection values;
        object?[] arguments;

        try
        {
            values = await CollectValuesAsync(request);
            arguments = ParameterBinder.Bind(route.Method, values);
        }
        catch (FormatException ex)
        {
            await new JsonResult(new { success = false, message = ex.Message }, 400)
                .ExecuteAsync(context.Response);
            return;
        }

        IHttpResult result;

        try
        {
            var controller = (BaseController)Activator.CreateInstance(route.ControllerType)!;
            controller.Initialize(context, _sessionService);

            object? actionResult = route.Method.Invoke(controller, arguments);

            if (actionResult is Task task)
            {
                await task;
                actionResult = ExtractTaskResult(task);
            }

            if (actionResult is not IHttpResult httpResult)
            {
                Console.WriteLine($"Метод {route.ControllerType.Name}.{route.Method.Name} вернул не IHttpResult.");
                await new JsonResult(new { success = false, message = "Некорректный результат действия." }, 500)
                    .ExecuteAsync(context.Response);
                return;
            }

            result = httpResult;
        }
        catch (TargetInvocationException ex)
        {
            Console.WriteLine($"Ошибка в контроллере: {ex.InnerException?.Message}");
            await new JsonResult(new { success = false, message = "Внутренняя ошибка сервера." }, 500)
                .ExecuteAsync(context.Response);
            return;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка выполнения действия: {ex.Message}");
            await new JsonResult(new { success = false, message = "Внутренняя ошибка сервера." }, 500)
                .ExecuteAsync(context.Response);
            return;
        }

        await result.ExecuteAsync(context.Response);
    }

    private bool IsAuthorized(HttpListenerContext context, Type controllerType, MethodInfo method)
    {
        bool requiresAuthorization =
            controllerType.IsDefined(typeof(AuthorizeAttribute), true) ||
            method.IsDefined(typeof(AuthorizeAttribute), true);

        if (!requiresAuthorization)
        {
            return true;
        }

        string? sessionId = context.Request.Cookies["sessionId"]?.Value;
        int? userId = _sessionService.GetUserId(sessionId);

        return userId.HasValue;
    }

    private static async Task<NameValueCollection> CollectValuesAsync(HttpListenerRequest request)
    {
        var query = request.QueryString;
        if (!request.HasEntityBody) return query;

        using var reader = new StreamReader(request.InputStream, request.ContentEncoding);
        string body = await reader.ReadToEndAsync();
        return FormDataParser.Merge(query, FormDataParser.Parse(body));
    }

    private static object? ExtractTaskResult(Task task)
    {
        Type taskType = task.GetType();

        if (!taskType.IsGenericType)
            return null;

        return taskType.GetProperty("Result")?.GetValue(task);
    }
}