using System.Net;

namespace MiniHttpServer.Framework.Results;

public sealed class RedirectResult : IHttpResult
{
    private readonly string _location;
    private readonly int _statusCode;

    public RedirectResult(string location, int statusCode = 303)
    {
        _location = string.IsNullOrWhiteSpace(location) ? "/" : location;
        _statusCode = statusCode;
    }

    public HttpResponseData Build()
    {
        return new HttpResponseData(_statusCode, "text/html; charset=utf-8", Array.Empty<byte>());
    }

    public async Task ExecuteAsync(HttpListenerResponse response, CancellationToken cancellationToken = default)
    {
        var data = Build();
        response.StatusCode = data.StatusCode;
        response.ContentType = data.ContentType;
        response.ContentLength64 = data.Body.Length;
        response.Headers["Server"] = "MiniHttpServer/1.0";
        response.RedirectLocation = _location;

        if (data.Body.Length > 0)
            await response.OutputStream.WriteAsync(data.Body, cancellationToken);
    }
}