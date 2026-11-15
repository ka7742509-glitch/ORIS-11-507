using System.Net;

namespace MiniHttpServer.Framework.Handlers;

public abstract class RequestHandler
{
    protected RequestHandler? Next { get; private set; }

    public RequestHandler SetNext(RequestHandler next)
    {
        Next = next;
        return next;
    }

    public abstract Task HandleAsync(HttpListenerContext context);

    protected async Task HandleNextAsync(HttpListenerContext context)
    {
        if (Next is not null)
            await Next.HandleAsync(context);
    }
}