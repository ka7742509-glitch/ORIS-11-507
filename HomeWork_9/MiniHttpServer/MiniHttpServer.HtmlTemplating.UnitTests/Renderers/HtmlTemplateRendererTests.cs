using MiniHttpServer.HtmlTemplating;

namespace MiniHttpServer.HtmlTemplating.UnitTests.Renderers;

[TestClass]
public class HtmlTemplateRendererTests
{
    private readonly HtmlTemplateRenderer _renderer = new();

    [TestMethod]
    public void RenderFromString_ValidProperty_ReplacesPlaceholder()
    {
        var model = new { Name = "Alex" };
        string result = _renderer.RenderFromString("<h1>${Name}</h1>", model);
        Assert.AreEqual("<h1>Alex</h1>", result);
    }

    [TestMethod]
    public void RenderFromString_MultipleProperties_ReplacesAllPlaceholders()
    {
        var model = new { Name = "Alex", Age = 20 };
        string result = _renderer.RenderFromString("${Name}:${Age}", model);
        Assert.AreEqual("Alex:20", result);
    }

    [TestMethod]
    public void RenderFromString_RepeatedProperty_ReplacesAllOccurrences()
    {
        var model = new { Name = "Alex" };
        string result = _renderer.RenderFromString("${Name}-${Name}", model);
        Assert.AreEqual("Alex-Alex", result);
    }

    [TestMethod]
    public void RenderFromString_NestedProperty_ResolvesChain()
    {
        var model = new { User = new { Name = "Alex" } };
        string result = _renderer.RenderFromString("${User.Name}", model);
        Assert.AreEqual("Alex", result);
    }

    [TestMethod]
    public void RenderFromString_EmptyTemplate_ReturnsEmptyString()
    {
        Assert.AreEqual("", _renderer.RenderFromString("", new { }));
    }

    [TestMethod]
    public void RenderFromString_ScalarModel_SubstitutesValue()
    {
        Assert.AreEqual("<p>Вася</p>", _renderer.RenderFromString("<p>${name}</p>", "Вася"));
    }

    [TestMethod]
    public void RenderFromString_TrueCondition_RendersIfBlock()
    {
        var model = new { IsActive = true };
        string result = _renderer.RenderFromString("$if(IsActive)DA$endif", model);
        Assert.AreEqual("DA", result);
    }

    [TestMethod]
    public void RenderFromString_FalseCondition_WithoutElse_ReturnsEmpty()
    {
        var model = new { IsActive = false };
        string result = _renderer.RenderFromString("$if(IsActive)DA$endif", model);
        Assert.AreEqual("", result);
    }

    [TestMethod]
    public void RenderFromString_FalseCondition_RendersElseBranch()
    {
        var model = new { IsActive = false };
        string result = _renderer.RenderFromString("$if(IsActive)DA$elseNET$endif", model);
        Assert.AreEqual("NET", result);
    }

    [TestMethod]
    public void RenderFromString_ComparisonCondition_EvaluatesCorrectly()
    {
        var model = new { Age = 20 };
        string result = _renderer.RenderFromString("$if(Age >= 18)OK$elseNO$endif", model);
        Assert.AreEqual("OK", result);
    }

    [TestMethod]
    public void RenderFromString_Collection_RendersAllItems()
    {
        var model = new { Items = new[] { new { Name = "A" }, new { Name = "B" } } };
        string result = _renderer.RenderFromString("$foreach(var i in Items)${i.Name}$endfor", model);
        Assert.AreEqual("AB", result);
    }

    [TestMethod]
    public void RenderFromString_EmptyCollection_ReturnsNoItems()
    {
        var model = new { Items = Array.Empty<object>() };
        string result = _renderer.RenderFromString("$foreach(var i in Items)X$endfor", model);
        Assert.AreEqual("", result);
    }

    [TestMethod]
    public void RenderFromString_NestedIf_ResolvesInnerCondition()
    {
        var model = new { A = true, B = false };
        string result = _renderer.RenderFromString("$if(A)$if(B)X$else Y$endif$else Z$endif", model);
        Assert.AreEqual(" Y", result);
    }

    [TestMethod]
    public void RenderFromString_NestedForeach_RendersInnerLoop()
    {
        var model = new { Groups = new[] { new { Users = new[] { new { Name = "A" }, new { Name = "B" } } } } };
        string result = _renderer.RenderFromString("$foreach(var g in Groups)$foreach(var u in g.Users)${u.Name}$endfor$endfor", model);
        Assert.AreEqual("AB", result);
    }

    [TestMethod]
    public void RenderFromString_IfInsideForeach_UsesItemProperty()
    {
        var model = new { Users = new[] { new { Name = "A", Active = true }, new { Name = "B", Active = false } } };
        string result = _renderer.RenderFromString("$foreach(var u in Users)$if(u.Active)${u.Name}+$else${u.Name}-$endif$endfor", model);
        Assert.AreEqual("A+B-", result);
    }

    [TestMethod]
    public void RenderFromString_UserValue_HtmlEncodes()
    {
        var model = new { Name = "<script>" };
        string result = _renderer.RenderFromString("${Name}", model);
        Assert.AreEqual("&lt;script&gt;", result);
    }

    [TestMethod]
    public void RenderFromString_MissingEndif_ThrowsTemplateException()
    {
        Assert.ThrowsExactly<TemplateRenderException>(() => _renderer.RenderFromString("$if(X)Y", new { X = true }));
    }

    [TestMethod]
    public void RenderFromString_MissingEndfor_ThrowsTemplateException()
    {
        Assert.ThrowsExactly<TemplateRenderException>(() => _renderer.RenderFromString("$foreach(var i in Items)X", new { Items = new[] { 1 } }));
    }

    [TestMethod]
    public void RenderFromString_BadForeachHeader_ThrowsTemplateException()
    {
        Assert.ThrowsExactly<TemplateRenderException>(() => _renderer.RenderFromString("$foreach(bad)X$endfor", new { }));
    }

    [TestMethod]
    public void RenderFromString_UnknownProperty_ThrowsTemplateException()
    {
        Assert.ThrowsExactly<TemplateRenderException>(() => _renderer.RenderFromString("${Missing}", new { Name = "A" }));
    }
}