using System.Net;
using System.Text.Json;

namespace MiniHttpServer.Framework.Results;

public sealed class JsonResult : IHttpResult
{
    private readonly object _data;
    private readonly int _statusCode;

    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public JsonResult(object data, int statusCode = 200)
    {
        _data = data;
        _statusCode = statusCode;
    }

    public async Task ExecuteAsync(HttpListenerResponse response, CancellationToken cancellationToken = default)
    {
        byte[] buffer = JsonSerializer.SerializeToUtf8Bytes(_data, Options);

        response.StatusCode = _statusCode;
        response.ContentType = "application/json; charset=utf-8";
        response.ContentLength64 = buffer.Length;
        response.Headers["Server"] = "MiniHttpServer/1.0";

        await response.OutputStream.WriteAsync(buffer, cancellationToken);
    }
}