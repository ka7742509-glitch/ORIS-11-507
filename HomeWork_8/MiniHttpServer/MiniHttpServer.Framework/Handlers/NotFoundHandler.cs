using System.Net;
using System.Text;

namespace MiniHttpServer.Framework.Handlers;

public sealed class NotFoundHandler : RequestHandler
{
    public override async Task HandleAsync(HttpListenerContext context)
    {
        string requestedPath = context.Request.Url?.LocalPath ?? "/";

        byte[] buffer = Encoding.UTF8.GetBytes(
            "<!DOCTYPE html><html lang=\"ru\"><head><meta charset=\"utf-8\">" +
            "<title>404 Not Found</title></head><body>" +
            "<h1>404 Not Found</h1>" +
            $"<p>Ресурс не найден: {WebUtility.HtmlEncode(requestedPath)}</p>" +
            "</body></html>");

        var response = context.Response;
        response.StatusCode = 404;
        response.ContentType = "text/html; charset=utf-8";
        response.ContentLength64 = buffer.Length;
        response.Headers["Server"] = "MiniHttpServer/1.0";

        await response.OutputStream.WriteAsync(buffer);
    }
}