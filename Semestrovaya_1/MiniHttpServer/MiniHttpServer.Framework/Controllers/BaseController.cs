using System.Net;
using MiniHttpServer.Framework.Authentication;
using MiniHttpServer.Framework.Results;

namespace MiniHttpServer.Framework.Controllers;

public abstract class BaseController
{
    protected HttpListenerContext Context { get; private set; } = null!;
    protected IUserSessionService SessionService { get; private set; } = null!;
    public void Initialize(HttpListenerContext context, IUserSessionService sessionService)
    {
        Context = context;
        SessionService = sessionService;
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

    protected IHttpResult Page(string templatePath, object model, int statusCode = 200)
    {
        return new PageResult(templatePath, model, statusCode);
    }
    
    protected IHttpResult Redirect(string location, int statusCode = 303)
    {
        return new RedirectResult(location, statusCode);
    }
}