using System.Text.Json;

namespace HttpServer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string settingsPath = Path.Combine(AppContext.BaseDirectory, "settings.json");
            if (!File.Exists(settingsPath))
            {
                settingsPath = "settings.json";
            }
    
            string json = await File.ReadAllTextAsync(settingsPath);
            var settings = JsonSerializer.Deserialize<ServerSettings>(json);
            
            if (settings == null || string.IsNullOrEmpty(settings.ServerAddress))
            {
                Console.WriteLine("Ошибка: не найден адрес сервера в settings.json");
                return;
            }
    
            string htmlPath = Path.Combine(AppContext.BaseDirectory, "search-engine.html");
            if (!File.Exists(htmlPath))
            {
                htmlPath = "search-engine.html";
            }
    
            if (!File.Exists(htmlPath))
            {
                Console.WriteLine($"Ошибка: файл {htmlPath} не найден");
                return;
            }
    
            var server = new HttpServer(settings.ServerAddress, htmlPath);
            server.Start();
    
            while (true)
            {
                string? input = Console.ReadLine();
                if (input?.Trim().ToLower() == "stop")
                {
                    server.Stop();
                    break;
                }
            }
        }
    }
    
    public class ServerSettings
    {
        public string ServerAddress { get; set; } = string.Empty;
    }
}
