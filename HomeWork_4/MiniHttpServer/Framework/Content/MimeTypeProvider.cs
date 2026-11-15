namespace MiniHttpServer.Framework.Content;

public static class MimeTypeProvider
{
    private static readonly Dictionary<string, string> MimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        [".html"] = "text/html; charset=utf-8",
        [".htm"] = "text/html; charset=utf-8",
        [".css"] = "text/css; charset=utf-8",
        [".js"] = "text/javascript; charset=utf-8",
        [".json"] = "application/json; charset=utf-8",
        [".png"] = "image/png",
        [".jpg"] = "image/jpeg",
        [".jpeg"] = "image/jpeg",
        [".svg"] = "image/svg+xml",
        [".gif"] = "image/gif",
        [".ico"] = "image/x-icon",
        [".webp"] = "image/webp",
        [".woff"] = "font/woff",
        [".woff2"] = "font/woff2",
        [".ttf"] = "font/ttf",
        [".pdf"] = "application/pdf",
        [".txt"] = "text/plain; charset=utf-8"
    };

    public static string GetMimeType(string filePath)
    {
        string extension = Path.GetExtension(filePath);
        return MimeTypes.TryGetValue(extension, out string? mime)
            ? mime
            : "application/octet-stream";
    }
}