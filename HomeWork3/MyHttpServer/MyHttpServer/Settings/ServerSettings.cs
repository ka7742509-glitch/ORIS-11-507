using System.Text.Json;

namespace MyHttpServer.Settings;

public class ServerSettings
{
    private static readonly Lazy<ServerSettings> _instance = new(() => LoadSettings());

    public static ServerSettings Instance => _instance.Value;

    public string Host { get; set; } = "127.0.0.1";
    public int Port { get; set; } = 8888;
    public string UrlPath { get; set; } = "/";
    public string StaticFolderPath { get; set; } = "static";
    public string DefaultPage { get; set; } = "index.html";

    public string BasePath => UrlPath.EndsWith("/") ? UrlPath : UrlPath + "/";

    public string Prefix => $"http://{Host}:{Port}{BasePath}";

    private ServerSettings() { }

    private static ServerSettings LoadSettings()
    {
        string path = System.IO.Path.Combine(AppContext.BaseDirectory, "settings.json");

        if (File.Exists(path))
        {
            try
            {
                string json = File.ReadAllText(path);
                var settings = JsonSerializer.Deserialize<ServerSettings>(json);
                if (settings != null) return settings;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка чтения settings.json: {ex.Message}. Используются значения по умолчанию.");
            }
        }

        return new ServerSettings();
    }
}