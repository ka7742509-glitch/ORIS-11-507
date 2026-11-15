using System.Net;

namespace MyHttpServer.Framework.Handlers;

public abstract class Handler
{
    protected Handler? NextHandler { get; private set; }

    public Handler SetNext(Handler handler)
    {
        NextHandler = handler;
        return handler;
    }

    public abstract Task<bool> HandleRequestAsync(HttpListenerContext context);

    protected async Task<bool> HandleNextAsync(HttpListenerContext context)
    {
        if (NextHandler != null)
            return await NextHandler.HandleRequestAsync(context);
        return false;
    }
}