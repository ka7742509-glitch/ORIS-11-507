using System.Net;
using System.Text;

namespace MiniHttpServer.Framework.Results;

public sealed class HtmlResult : IHttpResult
{
    private readonly string _html;
    private readonly int _statusCode;

    public HtmlResult(string html, int statusCode = 200)
    {
        _html = html ?? string.Empty;
        _statusCode = statusCode;
    }

    public async Task ExecuteAsync(HttpListenerResponse response, CancellationToken cancellationToken = default)
    {
        byte[] buffer = Encoding.UTF8.GetBytes(_html);

        response.StatusCode = _statusCode;
        response.ContentType = "text/html; charset=utf-8";
        response.ContentLength64 = buffer.Length;
        response.Headers["Server"] = "MiniHttpServer/1.0";

        await response.OutputStream.WriteAsync(buffer, cancellationToken);
    }
}