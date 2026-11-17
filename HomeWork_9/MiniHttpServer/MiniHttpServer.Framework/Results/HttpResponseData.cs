namespace MiniHttpServer.Framework.Results;

public sealed class HttpResponseData
{
    public int StatusCode { get; }
    public string ContentType { get; }
    public byte[] Body { get; }

    public HttpResponseData(int statusCode, string contentType, byte[] body)
    {
        StatusCode = statusCode;
        ContentType = contentType;
        Body = body;
    }
}