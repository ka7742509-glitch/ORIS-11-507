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

    public HttpResponseData Build()
    {
        return new HttpResponseData(_statusCode, "application/json; charset=utf-8", JsonSerializer.SerializeToUtf8Bytes(_data, Options));
    }

    public async Task ExecuteAsync(HttpListenerResponse response, CancellationToken cancellationToken = default)
    {
        var data = Build();
        response.StatusCode = data.StatusCode;
        response.ContentType = data.ContentType;
        response.ContentLength64 = data.Body.Length;
        response.Headers["Server"] = "MiniHttpServer/1.0";
        await response.OutputStream.WriteAsync(data.Body, cancellationToken);
    }
}