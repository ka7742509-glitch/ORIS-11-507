using MyHttpServer.Settings;

namespace MyHttpServer.Services;

public class StaticFileService
{
    private readonly string _staticRoot;

    public StaticFileService()
    {
        _staticRoot = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, ServerSettings.Instance.StaticFolderPath));
    }

    public string? ResolveFilePath(string localPath)
    {
        string basePath = ServerSettings.Instance.BasePath;

        if (!localPath.StartsWith(basePath))
            return null;

        string relativePath = localPath.Substring(basePath.Length);
        string relativeFile;

        if (relativePath.Length == 0 || relativePath.EndsWith("/"))
            relativeFile = Path.Combine(relativePath, ServerSettings.Instance.DefaultPage);
        else if (Path.HasExtension(relativePath))
            relativeFile = relativePath;
        else
            return null;

        string fullPath = Path.GetFullPath(Path.Combine(_staticRoot, relativeFile));

        if (!fullPath.StartsWith(_staticRoot))
            return null;

        return File.Exists(fullPath) ? fullPath : null;
    }

    public string? GetNotFoundPagePath()
    {
        string path = Path.Combine(_staticRoot, "404.html");
        return File.Exists(path) ? path : null;
    }

    public async Task<byte[]> ReadFileAsync(string filePath)
    {
        return await File.ReadAllBytesAsync(filePath);
    }
}