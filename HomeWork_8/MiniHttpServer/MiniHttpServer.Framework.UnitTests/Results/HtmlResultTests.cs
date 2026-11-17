using System.Text;
using MiniHttpServer.Framework.Results;

namespace MiniHttpServer.Framework.UnitTests.Results;

[TestClass]
public class HtmlResultTests
{
    [TestMethod]
    public void Build_DefaultStatus_Is200()
        => Assert.AreEqual(200, new HtmlResult("<h1>x</h1>").Build().StatusCode);

    [TestMethod]
    public void Build_CustomStatus_IsPreserved()
        => Assert.AreEqual(404, new HtmlResult("x", 404).Build().StatusCode);

    [TestMethod]
    public void Build_ContentType_IsHtmlUtf8()
        => Assert.AreEqual("text/html; charset=utf-8", new HtmlResult("x").Build().ContentType);

    [TestMethod]
    public void Build_Body_MatchesUtf8Bytes()
    {
        var data = new HtmlResult("<h1>Привет</h1>").Build();
        Assert.AreEqual("<h1>Привет</h1>", Encoding.UTF8.GetString(data.Body));
    }
}