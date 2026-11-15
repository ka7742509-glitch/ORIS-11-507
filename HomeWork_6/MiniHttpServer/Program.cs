using MiniHttpServer.Framework;
using MiniHttpServer.Framework.Handlers;

namespace MiniHttpServer;

class Program
{
    static void Main()
    {
        var loggingHandler = new LoggingHandler();
        var staticFilesHandler = new StaticFilesHandler();
        var controllersHandler = new ControllersHandler();
        var notFoundHandler = new NotFoundHandler();

        loggingHandler
            .SetNext(staticFilesHandler)
            .SetNext(controllersHandler)
            .SetNext(notFoundHandler);

        var server = new HttpServer(loggingHandler);
        server.Start();

        Console.WriteLine("Команды: start, stop, restart, exit");

        while (true)
        {
            string? command = Console.ReadLine()?.Trim().ToLower();

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