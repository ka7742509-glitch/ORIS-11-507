using System.Net;
using MiniHttpServer.Framework.Results;

namespace MiniHttpServer.Framework.Controllers;

public abstract class BaseController
{
    protected HttpListenerContext Context { get; private set; } = null!;

    internal void SetContext(HttpListenerContext context)
    {
        Context = context;
    }

    protected IHttpResult Html(string html, int statusCode = 200)
    {
        return new HtmlResult(html, statusCode);
    }

    protected IHttpResult Json(object data, int statusCode = 200)
    {
        return new JsonResult(data, statusCode);
    }

    protected IHttpResult Text(string text, int statusCode = 200)
    {
        return new TextResult(text, statusCode);
    }
}