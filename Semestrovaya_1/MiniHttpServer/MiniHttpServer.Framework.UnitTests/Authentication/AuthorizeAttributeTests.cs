using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MiniHttpServer.Framework.Attributes;
using MiniHttpServer.Framework.Controllers;
using MiniHttpServer.Framework.Results;

namespace MiniHttpServer.Framework.UnitTests.Authentication;

[Controller]
public class TestAuthController : BaseController
{
    [Get]
    public IHttpResult PublicMethod() => new HtmlResult("public");

    [Get]
    [Authorize]
    public IHttpResult ProtectedMethod() => new HtmlResult("protected");
}

[TestClass]
public class AuthorizeAttributeTests
{
    [TestMethod]
    public void PublicMethod_HasNoAuthorizeAttribute()
    {
        MethodInfo method = typeof(TestAuthController).GetMethod("PublicMethod")!;
        bool hasAttribute = method.IsDefined(typeof(AuthorizeAttribute), true);
        Assert.IsFalse(hasAttribute);
    }

    [TestMethod]
    public void ProtectedMethod_HasAuthorizeAttribute()
    {
        MethodInfo method = typeof(TestAuthController).GetMethod("ProtectedMethod")!;
        bool hasAttribute = method.IsDefined(typeof(AuthorizeAttribute), true);
        Assert.IsTrue(hasAttribute);
    }

    [TestMethod]
    public void AuthorizeAttribute_CanBeAppliedToClass()
    {
        var usage = typeof(AuthorizeAttribute).GetCustomAttribute<AttributeUsageAttribute>();
        Assert.IsNotNull(usage);
        Assert.IsTrue(usage.ValidOn.HasFlag(AttributeTargets.Class));
    }

    [TestMethod]
    public void AuthorizeAttribute_CanBeAppliedToMethod()
    {
        var usage = typeof(AuthorizeAttribute).GetCustomAttribute<AttributeUsageAttribute>();
        Assert.IsNotNull(usage);
        Assert.IsTrue(usage.ValidOn.HasFlag(AttributeTargets.Method));
    }
}