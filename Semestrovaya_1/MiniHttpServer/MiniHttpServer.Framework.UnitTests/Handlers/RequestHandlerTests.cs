using System.Net;
using MiniHttpServer.Framework.Handlers;

namespace MiniHttpServer.Framework.UnitTests.Handlers;

[TestClass]
public class RequestHandlerTests
{
    private sealed class RecordingHandler : RequestHandler
    {
        private readonly List<string> _log;
        private readonly string _name;
        private readonly bool _pass;
        public RecordingHandler(List<string> log, string name, bool pass) { _log = log; _name = name; _pass = pass; }
        public override Task HandleAsync(HttpListenerContext context)
        {
            _log.Add(_name);
            return _pass ? HandleNextAsync(context) : Task.CompletedTask;
        }
    }

    [TestMethod]
    public async Task Chain_ExecutesInOrder()
    {
        var log = new List<string>();
        var a = new RecordingHandler(log, "a", true);
        var b = new RecordingHandler(log, "b", true);
        var c = new RecordingHandler(log, "c", false);
        a.SetNext(b).SetNext(c);
        await a.HandleAsync(null!);
        CollectionAssert.AreEqual(new[] { "a", "b", "c" }, log);
    }

    [TestMethod]
    public async Task HandlerThatStops_ChainDoesNotContinue()
    {
        var log = new List<string>();
        var a = new RecordingHandler(log, "a", false);
        a.SetNext(new RecordingHandler(log, "b", true));
        await a.HandleAsync(null!);
        CollectionAssert.AreEqual(new[] { "a" }, log);
    }

    [TestMethod]
    public async Task NoNext_DoesNotThrow()
    {
        var log = new List<string>();
        var a = new RecordingHandler(log, "a", true);
        await a.HandleAsync(null!);
        CollectionAssert.AreEqual(new[] { "a" }, log);
    }

    [TestMethod]
    public void SetNext_ReturnsNext_ForFluentChaining()
    {
        var log = new List<string>();
        var a = new RecordingHandler(log, "a", true);
        var b = new RecordingHandler(log, "b", true);
        Assert.AreSame(b, a.SetNext(b));
    }
}