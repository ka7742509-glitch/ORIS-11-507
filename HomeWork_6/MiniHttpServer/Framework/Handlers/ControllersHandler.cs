using System.Collections.Specialized;
using System.Net;
using System.Reflection;
using MiniHttpServer.Framework.Configuration;
using MiniHttpServer.Framework.Controllers;
using MiniHttpServer.Framework.Results;
using MiniHttpServer.Framework.Routing;

namespace MiniHttpServer.Framework.Handlers;

public sealed class ControllersHandler : RequestHandler
{
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
        RouteInfo? route = RouteResolver.Resolve(relativePath, request.HttpMethod);

        if (route is null)
        {
            await HandleNextAsync(context);
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
            controller.SetContext(context);

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

    private static async Task<NameValueCollection> CollectValuesAsync(HttpListenerRequest request)
    {
        var values = new NameValueCollection();

        foreach (string? key in request.QueryString.AllKeys)
        {
            if (key is not null)
                values[key] = request.QueryString[key];
        }

        if (!request.HasEntityBody)
            return values;

        using var reader = new StreamReader(request.InputStream, request.ContentEncoding);
        string body = await reader.ReadToEndAsync();

        NameValueCollection form = System.Web.HttpUtility.ParseQueryString(body);

        foreach (string? key in form.AllKeys)
        {
            if (key is not null)
                values[key] = form[key];
        }

        return values;
    }

    private static object? ExtractTaskResult(Task task)
    {
        Type taskType = task.GetType();

        if (!taskType.IsGenericType)
            return null;

        return taskType.GetProperty("Result")?.GetValue(task);
    }
}