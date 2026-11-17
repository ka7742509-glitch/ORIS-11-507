using System.Text;
using MiniHttpServer.Framework.Results;
using MiniHttpServer.HtmlTemplating;

namespace MiniHttpServer.Framework.UnitTests.Results;

[TestClass]
public class PageResultTests
{
    private sealed class FakeRenderer : IHtmlTemplateRenderer
    {
        public string? LastPath;
        public object? LastModel;
        public string RenderFromString(string t, object m) => "<h1>fake</h1>";
        public string RenderFromFile(string p, object m) { LastPath = p; LastModel = m; return "<h1>fake</h1>"; }
        public string RenderToFile(string i, string o, object m) => "<h1>fake</h1>";
    }

    [TestMethod]
    public void Build_UsesInjectedRenderer_AndReturnsItsHtml()
    {
        var fake = new FakeRenderer();
        var model = new { Name = "Alex" };
        var data = new PageResult("User/Profile.html", model, 200, fake).Build();
        Assert.AreEqual("<h1>fake</h1>", Encoding.UTF8.GetString(data.Body));
        Assert.AreSame(model, fake.LastModel);
        StringAssert.Contains(fake.LastPath!, "Profile.html");
    }

    [TestMethod]
    public void Build_ContentType_IsHtml()
    {
        var data = new PageResult("t.html", new { }, 200, new FakeRenderer()).Build();
        Assert.AreEqual("text/html; charset=utf-8", data.ContentType);
    }
}