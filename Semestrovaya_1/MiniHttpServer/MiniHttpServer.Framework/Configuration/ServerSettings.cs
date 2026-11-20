namespace MiniHttpServer.Framework.Configuration;

public class ServerSettings
{
    public int Port { get; set; } = 8888;
    public string Path { get; set; } = "";
    public string Host { get; set; } = "localhost";
    public string PublicDirectoryPath { get; set; } = "static";
    public string DefaultPage { get; set; } = "index.html";
    public string ConnectionString { get; set; } = "";
    public string HomePagePath { get; set; } = "/cards/list";
    public string LoginPagePath { get; set; } = "/auth/login";
    public string AdminLogin { get; set; } = "admin";
    public string AdminPasswordHash { get; set; } = "";
    public string AdminDisplayName { get; set; } = "Администратор";

    public string BasePath
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Path)) return "/";
            return "/" + Path.Trim('/') + "/";
        }
    }

    public string Prefix => $"http://{Host}:{Port}{BasePath}";
}