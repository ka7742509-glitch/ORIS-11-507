using Microsoft.VisualStudio.TestTools.UnitTesting;
using MiniHttpServer.Services.Authentication;

namespace MiniHttpServer.Framework.UnitTests.Authentication;

[TestClass]
public class UserSessionServiceTests
{
    private UserSessionService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _service = new UserSessionService();
    }

    [TestMethod]
    public void CreateSession_ValidUserId_ReturnsNonEmptySessionId()
    {
        string sessionId = _service.CreateSession(1);
        Assert.IsFalse(string.IsNullOrWhiteSpace(sessionId));
        Assert.AreEqual(32, sessionId.Length);
    }

    [TestMethod]
    public void CreateSession_MultipleCalls_ReturnsUniqueIds()
    {
        string id1 = _service.CreateSession(1);
        string id2 = _service.CreateSession(2);
        Assert.AreNotEqual(id1, id2);
    }

    [TestMethod]
    public void GetUserId_ValidSession_ReturnsUserId()
    {
        string sessionId = _service.CreateSession(42);
        int? userId = _service.GetUserId(sessionId);
        Assert.AreEqual(42, userId);
    }

    [TestMethod]
    public void GetUserId_NullSessionId_ReturnsNull()
    {
        int? userId = _service.GetUserId(null);
        Assert.IsNull(userId);
    }

    [TestMethod]
    public void GetUserId_EmptySessionId_ReturnsNull()
    {
        int? userId = _service.GetUserId("");
        Assert.IsNull(userId);
    }

    [TestMethod]
    public void GetUserId_UnknownSessionId_ReturnsNull()
    {
        int? userId = _service.GetUserId("unknown-session-id");
        Assert.IsNull(userId);
    }

    [TestMethod]
    public void RemoveSession_ValidSession_RemovesSession()
    {
        string sessionId = _service.CreateSession(1);
        _service.RemoveSession(sessionId);
        int? userId = _service.GetUserId(sessionId);
        Assert.IsNull(userId);
    }

    [TestMethod]
    public void RemoveSession_NullSessionId_DoesNotThrow()
    {
        _service.RemoveSession(null);
    }

    [TestMethod]
    public void RemoveSession_UnknownSessionId_DoesNotThrow()
    {
        _service.RemoveSession("unknown-session-id");
    }
}