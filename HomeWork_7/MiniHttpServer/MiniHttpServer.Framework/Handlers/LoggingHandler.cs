using System.Net;

namespace MiniHttpServer.Framework.Handlers;

public sealed class LoggingHandler : RequestHandler
{
    public override async Task HandleAsync(HttpListenerContext context)
    {
        var request = context.Request;
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {request.HttpMethod} {request.Url?.LocalPath}");

        await HandleNextAsync(context);
    }
}