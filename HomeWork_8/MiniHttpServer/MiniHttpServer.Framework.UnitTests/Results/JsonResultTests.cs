using System.Text.Json;
using MiniHttpServer.Framework.Results;

namespace MiniHttpServer.Framework.UnitTests.Results;

[TestClass]
public class JsonResultTests
{
    [TestMethod]
    public void Build_ContentType_IsJsonUtf8()
        => Assert.AreEqual("application/json; charset=utf-8", new JsonResult(new { }).Build().ContentType);

    [TestMethod]
    public void Build_SerializesCamelCase()
    {
        var data = new JsonResult(new { Success = true, Message = "ok" }).Build();
        using var doc = JsonDocument.Parse(data.Body);
        Assert.IsTrue(doc.RootElement.GetProperty("success").GetBoolean());
        Assert.AreEqual("ok", doc.RootElement.GetProperty("message").GetString());
    }

    [TestMethod]
    public void Build_CustomStatus_IsPreserved()
        => Assert.AreEqual(400, new JsonResult(new { }, 400).Build().StatusCode);
}