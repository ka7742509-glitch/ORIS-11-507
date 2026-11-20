using System.Collections.Specialized;
using System.Net;
using System.Reflection;
using MiniHttpServer.Framework.Attributes;
using MiniHttpServer.Framework.Authentication;
using MiniHttpServer.Framework.Configuration;
using MiniHttpServer.Framework.Controllers;
using MiniHttpServer.Framework.Results;
using MiniHttpServer.Framework.Routing;

namespace MiniHttpServer.Framework.Handlers;

public sealed class ControllersHandler : RequestHandler
{
    private const int MaxBodyLength = 1_000_000;

    private readonly IUserSessionService _sessionService;
    private readonly RouteResolver _routeResolver;

    public ControllersHandler(IUserSessionService sessionService, RouteResolver? routeResolver = null)
    {
        _sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
        _routeResolver = routeResolver ?? RouteResolver.Default;
    }

    public override async Task HandleAsync(HttpListenerContext context)
    {
        var request = context.Request;
        string localPath = request.Url?.LocalPath ?? string.Empty;
        string basePath = ConfigurationManager.Instance.Settings.BasePath;

        if (!localPath.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
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
            await SendErrorAsync(context, 401, "Требуется аутентификация");
            return;
        }

        object?[] arguments;

        try
        {
            NameValueCollection values = await CollectValuesAsync(request);
            arguments = ParameterBinder.Bind(route.Method, values);
        }
        catch (FormatException ex)
        {
            Console.WriteLine($"[400] Ошибка параметров: {ex.Message}");
            await SendErrorAsync(context, 400, "Некорректные данные запроса");
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
                Console.WriteLine($"[500] {route.ControllerType.Name}.{route.Method.Name} вернул не IHttpResult.");
                await SendErrorAsync(context, 500, "Внутренняя ошибка сервера");
                return;
            }

            result = httpResult;
        }
        catch (TargetInvocationException ex)
        {
            Exception inner = ex.InnerException ?? ex;
            Console.WriteLine($"[ERROR] {route.ControllerType.Name}.{route.Method.Name}: {inner.GetType().Name}: {inner.Message}");

            int code = MapExceptionToStatusCode(inner);
            await SendErrorAsync(context, code, GetPublicMessage(code));
            return;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Выполнение действия: {ex.GetType().Name}: {ex.Message}");

            int code = MapExceptionToStatusCode(ex);
            await SendErrorAsync(context, code, GetPublicMessage(code));
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
            return true;

        string? sessionId = context.Request.Cookies["sessionId"]?.Value;
        return _sessionService.GetUserId(sessionId).HasValue;
    }
    
    private static int MapExceptionToStatusCode(Exception ex)
    {
        string typeName = ex.GetType().Name;

        if (typeName == "NpgsqlException")
            return 503;

        if (typeName == "PostgresException")
        {
            string? sqlState = ex.GetType().GetProperty("SqlState")?.GetValue(ex) as string;

            if (sqlState is not null && sqlState.StartsWith("23"))
                return 409;

            return 500;
        }

        return typeName switch
        {
            "FormatException" or "ArgumentException" or "ArgumentNullException" => 400,
            "SqlException" => 503,
            _ => 500
        };
    }

    private static string GetPublicMessage(int code)
    {
        return code switch
        {
            400 => "Некорректные данные запроса",
            401 => "Требуется аутентификация",
            409 => "Такая запись уже существует",
            503 => "База данных временно недоступна",
            _ => "Внутренняя ошибка сервера"
        };
    }

    private static async Task<NameValueCollection> CollectValuesAsync(HttpListenerRequest request)
    {
        NameValueCollection query = request.QueryString;

        if (!request.HasEntityBody)
            return query;

        if (request.ContentLength64 > MaxBodyLength)
            throw new FormatException("Тело запроса превышает допустимый размер.");

        using var reader = new StreamReader(request.InputStream, request.ContentEncoding);
        string body = await reader.ReadToEndAsync();

        if (body.Length > MaxBodyLength)
            throw new FormatException("Тело запроса превышает допустимый размер.");

        return FormDataParser.Merge(query, FormDataParser.Parse(body));
    }

    private static object? ExtractTaskResult(Task task)
    {
        Type taskType = task.GetType();

        if (!taskType.IsGenericType)
            return null;

        return taskType.GetProperty("Result")?.GetValue(task);
    }

    private static async Task SendErrorAsync(HttpListenerContext context, int code, string message)
    {
        try
        {
            string accept = context.Request.Headers["Accept"] ?? string.Empty;
            bool wantsHtml = accept.Contains("text/html", StringComparison.OrdinalIgnoreCase);

            IHttpResult result = wantsHtml
                ? new HtmlResult(BuildErrorHtml(code, message), code)
                : new JsonResult(new { success = false, message }, code);

            await result.ExecuteAsync(context.Response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WARN] Не удалось отправить ответ {code}: {ex.Message}");
        }
    }
    private static string BuildErrorHtml(int code, string message)
    {
        var settings = ConfigurationManager.Instance.Settings;
        string safeMessage = WebUtility.HtmlEncode(message);

        string links = string.Empty;

        if (code == 401)
            links += $"<p><a href=\"{settings.LoginPagePath}\">Войти в панель управления</a></p>";

        links += $"<p><a href=\"{settings.HomePagePath}\">Вернуться на главную</a></p>";

        return "<!DOCTYPE html><html lang=\"ru\"><head><meta charset=\"utf-8\">" +
               $"<title>{code}</title></head><body>" +
               $"<h1>{code}</h1><p>{safeMessage}</p>" +
               links +
               "</body></html>";
    }
}