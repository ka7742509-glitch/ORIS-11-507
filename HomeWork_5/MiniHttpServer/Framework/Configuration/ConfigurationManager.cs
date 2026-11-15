using System.Text.Json;

namespace MiniHttpServer.Framework.Configuration;

public sealed class ConfigurationManager
{
    private static readonly Lazy<ConfigurationManager> _instance = new(() => new ConfigurationManager());

    public static ConfigurationManager Instance => _instance.Value;

    public ServerSettings Settings { get; }

    private ConfigurationManager()
    {
        Settings = Load();
    }

    private static ServerSettings Load()
    {
        string path = System.IO.Path.Combine(AppContext.BaseDirectory, "settings.json");

        if (!File.Exists(path))
            return new ServerSettings();

        try
        {
            string json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<ServerSettings>(json) ?? new ServerSettings();
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Ошибка чтения settings.json: {ex.Message}. Используются значения по умолчанию.");
            return new ServerSettings();
        }
    }
}