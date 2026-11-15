using System.Collections.Specialized;
using System.Net;
using System.Reflection;
using System.Text;
using MiniHttpServer.Framework.Configuration;
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
            await WriteTextAsync(context.Response, 400, $"Bad Request: {ex.Message}");
            return;
        }

        object? result;

        try
        {
            object controller = Activator.CreateInstance(route.ControllerType)!;
            result = route.Method.Invoke(controller, arguments);

            if (result is Task task)
            {
                await task;
                result = ExtractTaskResult(task);
            }
        }
        catch (TargetInvocationException ex)
        {
            Console.WriteLine($"Ошибка в контроллере: {ex.InnerException?.Message}");
            await WriteTextAsync(context.Response, 500, "Internal Server Error");
            return;
        }

        await WriteTextAsync(context.Response, 200, result?.ToString() ?? string.Empty);
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

    private static async Task WriteTextAsync(HttpListenerResponse response, int code, string text)
    {
        byte[] buffer = Encoding.UTF8.GetBytes(text);

        response.StatusCode = code;
        response.ContentType = "text/plain; charset=utf-8";
        response.ContentLength64 = buffer.Length;
        response.Headers["Server"] = "MiniHttpServer/1.0";

        await response.OutputStream.WriteAsync(buffer);
    }
}