using System.Net;
using System.Reflection;
using System.Web;
using MyHttpServer.Framework.Attributes;

namespace MyHttpServer.Framework.Handlers;

public class ControllerHandler : Handler
{
    private readonly Assembly _assembly;

    public ControllerHandler()
    {
        _assembly = Assembly.GetExecutingAssembly();
    }

    public override async Task<bool> HandleRequestAsync(HttpListenerContext context)
    {
        string localPath = context.Request.Url!.LocalPath.Trim('/');
        string[] segments = localPath.Split('/');

        if (segments.Length < 2)
            return await HandleNextAsync(context);

        string controllerName = segments[0];
        string methodName = segments[1];
        string httpMethod = context.Request.HttpMethod;

        var controllerType = _assembly.GetTypes()
            .FirstOrDefault(t =>
            {
                var attr = t.GetCustomAttribute<ControllerAttribute>();
                return attr != null && attr.Name.ToLower() == controllerName.ToLower();
            });

        if (controllerType == null)
            return await HandleNextAsync(context);

        var method = controllerType.GetMethods()
            .FirstOrDefault(m =>
            {
                var attrs = m.GetCustomAttributes(true);
                if (httpMethod == "GET")
                    return attrs.Any(a => a is GetAttribute getAttr && getAttr.Route.ToLower() == methodName.ToLower());
                if (httpMethod == "POST")
                    return attrs.Any(a => a is PostAttribute postAttr && postAttr.Route.ToLower() == methodName.ToLower());
                return false;
            });

        if (method == null)
            return await HandleNextAsync(context);

        var parameters = method.GetParameters();
        var queryParams = new object[parameters.Length];

        var allParams = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (string key in context.Request.QueryString.AllKeys)
        {
            if (key != null)
                allParams[key] = context.Request.QueryString[key]!;
        }

        if (httpMethod == "POST" && context.Request.HasEntityBody)
        {
            using var reader = new StreamReader(context.Request.InputStream);
            string body = await reader.ReadToEndAsync();
            var formData = HttpUtility.ParseQueryString(body);
            foreach (string key in formData.AllKeys)
            {
                if (key != null)
                    allParams[key] = formData[key]!;
            }
        }

        for (int i = 0; i < parameters.Length; i++)
        {
            string paramName = parameters[i].Name!;
            if (allParams.TryGetValue(paramName, out string? value))
            {
                queryParams[i] = Convert.ChangeType(value, parameters[i].ParameterType);
            }
            else
            {
                queryParams[i] = parameters[i].DefaultValue ??
                    Activator.CreateInstance(parameters[i].ParameterType)!;
            }
        }

        try
        {
            var controller = Activator.CreateInstance(controllerType);
            method.Invoke(controller, queryParams);

            var response = context.Response;
            response.StatusCode = 200;
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes("OK");
            response.ContentType = "text/plain; charset=utf-8";
            response.ContentLength64 = buffer.Length;

            using var output = response.OutputStream;
            await output.WriteAsync(buffer);
            await output.FlushAsync();

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка в контроллере: {ex.Message}");
            var response = context.Response;
            response.StatusCode = (int)HttpStatusCode.InternalServerError;

            byte[] buffer = System.Text.Encoding.UTF8.GetBytes($"500 Internal Server Error: {ex.Message}");
            response.ContentType = "text/plain; charset=utf-8";
            response.ContentLength64 = buffer.Length;

            using var output = response.OutputStream;
            await output.WriteAsync(buffer);
            await output.FlushAsync();

            return true;
        }
    }
}