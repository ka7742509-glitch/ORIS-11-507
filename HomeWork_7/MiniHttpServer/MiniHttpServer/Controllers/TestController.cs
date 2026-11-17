using MiniHttpServer.Framework.Attributes;
using MiniHttpServer.Framework.Controllers;
using MiniHttpServer.Framework.Results;

namespace MiniHttpServer.Controllers;

[Controller]
public class TestController : BaseController
{
    [Get]
    public IHttpResult Index()
    {
        return Html("<h1>Hello from controller!</h1>");
    }

    [Get]
    public IHttpResult Hello()
    {
        return Html("<h1>Hello World!</h1>");
    }

    [Get]
    public IHttpResult Info()
    {
        return Json(new
        {
            Id = 1,
            Message = "Hello World!",
            Success = true
        });
    }

    [Post]
    public IHttpResult Submit()
    {
        return Json(new { success = true, message = "POST request processed" });
    }

    [Get]
    public IHttpResult Details(int id, string name)
    {
        return Json(new { id, name });
    }

    [Post]
    public IHttpResult Echo(string demoUser, string message)
    {
        return Json(new { demoUser, message });
    }

    [Get]
    public IHttpResult RequestInfo()
    {
        return Json(new
        {
            method = Context.Request.HttpMethod,
            path = Context.Request.Url?.LocalPath ?? "",
            userAgent = Context.Request.Headers["User-Agent"] ?? "unknown"
        });
    }
}