using MiniHttpServer.Framework;
using MiniHttpServer.Framework.Handlers;
using MiniHttpServer.Framework.Services.Authentication;

namespace MiniHttpServer;

class Program
{
    static async Task Main(string[] args)
    {
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            Console.WriteLine($"[FATAL] {e.ExceptionObject}");
        };

        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            Console.WriteLine($"[TASK] {e.Exception?.Message}");
            e.SetObserved();
        };

        var sessionService = new UserSessionService();

        var logging = new LoggingHandler();
        var staticFiles = new StaticFilesHandler();
        var controllers = new ControllersHandler(sessionService);
        var notFound = new NotFoundHandler();

        logging
            .SetNext(staticFiles)
            .SetNext(controllers)
            .SetNext(notFound);

        var server = new HttpServer(logging);
        server.Start();

        Console.WriteLine("Команды: start, stop, restart, exit");

        while (true)
        {
            string? line = Console.ReadLine();
            if (line is null)
            {
                Thread.Sleep(Timeout.Infinite);
                continue;
            }
            string command = line.Trim().ToLower();
            
            switch (command)
            {
                case "start": server.Start(); break;
                case "stop": server.Stop(); break;
                case "restart": server.Stop(); server.Start(); break;
                case "exit": server.Stop(); return;
                default: Console.WriteLine("Команды: start, stop, restart, exit"); break;
            }
        }
    }
}