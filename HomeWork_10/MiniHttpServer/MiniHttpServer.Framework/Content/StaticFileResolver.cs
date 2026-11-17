namespace MiniHttpServer.Framework.Content;

public static class StaticFileResolver
{
    public static string? Resolve(string localPath, string basePath, string staticRoot, string defaultPage)
    {
        if (!localPath.StartsWith(basePath)) return null;

        string relative = localPath.Substring(basePath.Length).Replace('/', Path.DirectorySeparatorChar);
        string full = Path.GetFullPath(Path.Combine(staticRoot, relative));

        if (!full.StartsWith(staticRoot, StringComparison.OrdinalIgnoreCase)) return null;

        if (relative.Length == 0 || localPath.EndsWith("/"))
            full = Path.Combine(full, defaultPage);
        else if (!Path.HasExtension(full))
            return null;

        return File.Exists(full) ? full : null;
    }
}