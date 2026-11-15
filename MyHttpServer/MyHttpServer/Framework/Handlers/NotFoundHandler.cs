using System.Net;
using System.Text;

namespace MyHttpServer.Framework.Handlers;

public class NotFoundHandler : Handler
{
    private readonly string? _notFoundPagePath;

    public NotFoundHandler(string? notFoundPagePath)
    {
        _notFoundPagePath = notFoundPagePath;
    }

    public override async Task<bool> HandleRequestAsync(HttpListenerContext context)
    {
        var response = context.Response;
        response.StatusCode = (int)HttpStatusCode.NotFound;

        if (_notFoundPagePath != null && File.Exists(_notFoundPagePath))
        {
            byte[] buffer = await File.ReadAllBytesAsync(_notFoundPagePath);
            response.ContentType = "text/html; charset=utf-8";
            response.ContentLength64 = buffer.Length;

            using var output = response.OutputStream;
            await output.WriteAsync(buffer);
            await output.FlushAsync();
        }
        else
        {
            byte[] buffer = Encoding.UTF8.GetBytes("<h1>404 Not Found</h1>");
            response.ContentType = "text/html; charset=utf-8";
            response.ContentLength64 = buffer.Length;

            using var output = response.OutputStream;
            await output.WriteAsync(buffer);
            await output.FlushAsync();
        }

        return true;
    }
}