using MiniHttpServer.Framework.Content;

namespace MiniHttpServer.Framework.UnitTests.Content;

[TestClass]
public class StaticFileResolverTests
{
    private string _root = null!;

    [TestInitialize]
    public void Setup()
    {
        _root = Path.Combine(Path.GetTempPath(), "static_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(_root, "site"));
        File.WriteAllText(Path.Combine(_root, "site", "index.html"), "x");
        File.WriteAllText(Path.Combine(_root, "site", "style.css"), "x");
        File.WriteAllText(Path.Combine(_root, "secret.txt"), "x");
    }

    [TestCleanup]
    public void Cleanup() => Directory.Delete(_root, true);

    [TestMethod]
    public void Resolve_Directory_ReturnsIndex()
    {
        string? r = StaticFileResolver.Resolve("/site/", "/", _root, "index.html");
        Assert.AreEqual(Path.Combine(_root, "site", "index.html"), r);
    }

    [TestMethod]
    public void Resolve_ExistingFile_ReturnsPath()
    {
        string? r = StaticFileResolver.Resolve("/site/style.css", "/", _root, "index.html");
        Assert.AreEqual(Path.Combine(_root, "site", "style.css"), r);
    }

    [TestMethod]
    public void Resolve_MissingFile_ReturnsNull()
        => Assert.IsNull(StaticFileResolver.Resolve("/site/no.css", "/", _root, "index.html"));

    [TestMethod]
    public void Resolve_NoExtensionNoSlash_ReturnsNull()
        => Assert.IsNull(StaticFileResolver.Resolve("/site", "/", _root, "index.html"));

    [TestMethod]
    public void Resolve_Traversal_ReturnsNull()
        => Assert.IsNull(StaticFileResolver.Resolve("/../secret.txt", "/", _root, "index.html"));
}