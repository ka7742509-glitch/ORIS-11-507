using MiniHttpServer.Framework.Content;

namespace MiniHttpServer.Framework.UnitTests.Content;

[TestClass]
public class MimeTypeProviderTests
{
    [DataTestMethod]
    [DataRow("a.html", "text/html; charset=utf-8")]
    [DataRow("a.css", "text/css; charset=utf-8")]
    [DataRow("a.js", "text/javascript; charset=utf-8")]
    [DataRow("a.png", "image/png")]
    [DataRow("a.json", "application/json; charset=utf-8")]
    public void GetMimeType_KnownExtension_ReturnsType(string file, string expected)
        => Assert.AreEqual(expected, MimeTypeProvider.GetMimeType(file));

    [TestMethod]
    public void GetMimeType_UnknownExtension_ReturnsOctetStream()
        => Assert.AreEqual("application/octet-stream", MimeTypeProvider.GetMimeType("a.xyz"));

    [TestMethod]
    public void GetMimeType_UpperCaseExtension_StillResolves()
        => Assert.AreEqual("image/png", MimeTypeProvider.GetMimeType("a.PNG"));
}