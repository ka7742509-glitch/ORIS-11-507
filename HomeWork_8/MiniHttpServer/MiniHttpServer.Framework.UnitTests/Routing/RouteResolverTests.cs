using MiniHttpServer.Framework.Routing;

namespace MiniHttpServer.Framework.UnitTests.Routing;

[TestClass]
public class RouteResolverTests
{
    private readonly RouteResolver _resolver = new(typeof(RouteResolverTests).Assembly);

    [TestMethod]
    public void Resolve_ValidGetRoute_ReturnsMethod()
    {
        var route = _resolver.Resolve("/sample/index", "GET");
        Assert.IsNotNull(route);
        Assert.AreEqual("Index", route.Method.Name);
    }

    [TestMethod]
    public void Resolve_PostRoute_RequiresPostAttribute()
    {
        Assert.IsNotNull(_resolver.Resolve("/sample/submit", "POST"));
        Assert.IsNull(_resolver.Resolve("/sample/submit", "GET"));
    }

    [TestMethod]
    public void Resolve_MethodWithoutAttribute_ReturnsNull()
        => Assert.IsNull(_resolver.Resolve("/sample/noattribute", "GET"));

    [TestMethod]
    public void Resolve_ClassWithoutControllerAttribute_ReturnsNull()
        => Assert.IsNull(_resolver.Resolve("/nota/index", "GET"));

    [TestMethod]
    public void Resolve_UnknownController_ReturnsNull()
        => Assert.IsNull(_resolver.Resolve("/ghost/index", "GET"));

    [TestMethod]
    public void Resolve_CaseInsensitive_Resolves()
        => Assert.IsNotNull(_resolver.Resolve("/SAMPLE/INDEX", "get"));

    [TestMethod]
    public void Resolve_WrongSegmentCount_ReturnsNull()
    {
        Assert.IsNull(_resolver.Resolve("/sample", "GET"));
        Assert.IsNull(_resolver.Resolve("/a/b/c", "GET"));
    }
}