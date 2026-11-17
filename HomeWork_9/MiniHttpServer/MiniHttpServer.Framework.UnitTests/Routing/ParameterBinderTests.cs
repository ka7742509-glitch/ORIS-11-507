using System.Collections.Specialized;
using System.Reflection;
using MiniHttpServer.Framework.Routing;

namespace MiniHttpServer.Framework.UnitTests.Routing;

[TestClass]
public class ParameterBinderTests
{
    private static MethodInfo Method(string name) => typeof(ParameterBinderTests).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static)!;

    private static void Target(int id, string name) { }
    private static void WithDefault(int id, string name = "Guest") { }
    private static void BoolTarget(bool flag) { }

    [TestMethod]
    public void Bind_ValidValues_ConvertsTypes()
    {
        var values = new NameValueCollection { ["id"] = "15", ["name"] = "Alex" };
        object?[] args = ParameterBinder.Bind(Method(nameof(Target)), values);
        Assert.AreEqual(15, args[0]);
        Assert.AreEqual("Alex", args[1]);
    }

    [TestMethod]
    public void Bind_MissingRequired_ThrowsFormat()
    {
        var values = new NameValueCollection { ["id"] = "15" };
        Assert.ThrowsExactly<FormatException>(() => ParameterBinder.Bind(Method(nameof(Target)), values));
    }

    [TestMethod]
    public void Bind_InvalidInt_ThrowsFormat()
    {
        var values = new NameValueCollection { ["id"] = "abc", ["name"] = "Alex" };
        Assert.ThrowsExactly<FormatException>(() => ParameterBinder.Bind(Method(nameof(Target)), values));
    }

    [TestMethod]
    public void Bind_DefaultValue_UsedWhenMissing()
    {
        var values = new NameValueCollection { ["id"] = "1" };
        object?[] args = ParameterBinder.Bind(Method(nameof(WithDefault)), values);
        Assert.AreEqual("Guest", args[1]);
    }

    [TestMethod]
    public void Bind_Bool_Converts()
    {
        var values = new NameValueCollection { ["flag"] = "true" };
        object?[] args = ParameterBinder.Bind(Method(nameof(BoolTarget)), values);
        Assert.AreEqual(true, args[0]);
    }
}