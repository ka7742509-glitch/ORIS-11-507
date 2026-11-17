using MiniHttpServer.HtmlTemplating;

namespace MiniHttpServer.HtmlTemplating.UnitTests.Renderers;

[TestClass]
public class HtmlTemplateRendererFileTests
{
    private readonly HtmlTemplateRenderer _renderer = new();
    private string _dir = null!;

    [TestInitialize]
    public void Setup()
    {
        _dir = Path.Combine(Path.GetTempPath(), "tpl_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_dir);
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(_dir)) Directory.Delete(_dir, true);
    }

    [TestMethod]
    public void RenderFromFile_ValidFile_RendersContent()
    {
        string path = Path.Combine(_dir, "t.html");
        File.WriteAllText(path, "<p>${Name}</p>");
        string result = _renderer.RenderFromFile(path, new { Name = "Alex" });
        Assert.AreEqual("<p>Alex</p>", result);
    }

    [TestMethod]
    public void RenderFromFile_MissingFile_ThrowsFileNotFound()
    {
        Assert.ThrowsExactly<FileNotFoundException>(() =>
            _renderer.RenderFromFile(Path.Combine(_dir, "no.html"), new { }));
    }

    [TestMethod]
    public void RenderToFile_WritesResult_AndCreatesDirectory()
    {
        string input = Path.Combine(_dir, "in.html");
        string output = Path.Combine(_dir, "sub", "out.html");
        File.WriteAllText(input, "${Name}");
        string result = _renderer.RenderToFile(input, output, new { Name = "Alex" });
        Assert.AreEqual("Alex", result);
        Assert.AreEqual("Alex", File.ReadAllText(output));
    }
}