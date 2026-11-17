using MiniHttpServer.Framework.Attributes;
using MiniHttpServer.Framework.Controllers;
using MiniHttpServer.Framework.Results;

namespace MiniHttpServer.Framework.UnitTests.Routing;

[Controller]
public class SampleController : BaseController
{
    [Get] public IHttpResult Index() => new HtmlResult("");
    [Post] public IHttpResult Submit() => new HtmlResult("");
    public IHttpResult NoAttribute() => new HtmlResult("");
}

public class NotAController : BaseController
{
    [Get] public IHttpResult Index() => new HtmlResult("");
}