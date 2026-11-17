using System.Collections.Specialized;
using MiniHttpServer.Framework.Routing;

namespace MiniHttpServer.Framework.UnitTests.Routing;

[TestClass]
public class FormDataParserTests
{
    [TestMethod]
    public void Parse_UrlEncoded_DecodesPlusAndPercent()
    {
        var form = FormDataParser.Parse("message=Hello+world&email=a%40b.com");
        Assert.AreEqual("Hello world", form["message"]);
        Assert.AreEqual("a@b.com", form["email"]);
    }

    [TestMethod]
    public void Merge_FormOverridesQuery()
    {
        var query = new NameValueCollection { ["name"] = "fromQuery" };
        var form = new NameValueCollection { ["name"] = "fromBody" };
        var merged = FormDataParser.Merge(query, form);
        Assert.AreEqual("fromBody", merged["name"]);
    }
}