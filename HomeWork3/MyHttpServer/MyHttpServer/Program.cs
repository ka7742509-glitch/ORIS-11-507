using MyHttpServer.Core;

class Program
{
    static void Main()
    {
        var server = new HttpServer();
        server.Start();

        Console.WriteLine("Команды: start, stop, restart, exit");

        while (true)
        {
            string? command = Console.ReadLine()?.Trim().ToLower();

            switch (command)
            {
                case "start":
                    server.Start();
                    break;
                case "stop":
                    server.Stop();
                    break;
                case "restart":
                    server.Stop();
                    server.Start();
                    break;
                case "exit":
                    server.Stop();
                    return;
                default:
                    Console.WriteLine("Команды: start, stop, restart, exit");
                    break;
            }
        }
    }
}