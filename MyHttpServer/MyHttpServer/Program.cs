using MyHttpServer.Http;

namespace MyHttpServer;

class Program
{
    static void Main(string[] args)
    {
        var server = new HttpServer();
        server.Start();

        Console.WriteLine("Команды: stop, exit");

        while (true)
        {
            string? command = Console.ReadLine()?.Trim().ToLower();

            switch (command)
            {
                case "stop":
                    server.Stop();
                    break;
                case "exit":
                    server.Stop();
                    return;
                default:
                    Console.WriteLine("Команды: stop, exit");
                    break;
            }
        }
    }
}