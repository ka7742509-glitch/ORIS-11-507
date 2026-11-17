using System.Net;
using System.Text;

namespace MiniHttpServer.Framework.Results;

public sealed class TextResult : IHttpResult
{
    private readonly string _text;
    private readonly int _statusCode;

    public TextResult(string text, int statusCode = 200)
    {
        _text = text ?? string.Empty;
        _statusCode = statusCode;
    }

    public HttpResponseData Build()
    {
        return new HttpResponseData(_statusCode, "text/plain; charset=utf-8", Encoding.UTF8.GetBytes(_text));
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