using System.Net;

namespace MyHttpServer.Framework.Handlers;

public class StaticFileHandler : Handler
{
    private readonly string _staticFolderPath;

    public StaticFileHandler(string staticFolderPath)
    {
        _staticFolderPath = staticFolderPath;
    }

    public override async Task<bool> HandleRequestAsync(HttpListenerContext context)
    {
        string localPath = context.Request.Url!.LocalPath;
        string filePath;

        if (localPath.EndsWith("/"))
        {
            filePath = Path.Combine(_staticFolderPath, localPath.TrimStart('/'), "index.html");
        }
        else if (Path.HasExtension(localPath))
        {
            filePath = Path.Combine(_staticFolderPath, localPath.TrimStart('/'));
        }
        else
        {
            return await HandleNextAsync(context);
        }

        if (!File.Exists(filePath))
            return await HandleNextAsync(context);

        await SendFileAsync(context, filePath);
        return true;
    }

    private async Task SendFileAsync(HttpListenerContext context, string filePath)
    {
        var response = context.Response;
        byte[] buffer = await File.ReadAllBytesAsync(filePath);

        string extension = Path.GetExtension(filePath).ToLower();
        response.ContentType = GetContentType(extension);
        response.ContentLength64 = buffer.Length;

        using var output = response.OutputStream;
        await output.WriteAsync(buffer);
        await output.FlushAsync();
    }

    private string GetContentType(string extension)
    {
        return extension switch
        {
            ".html" or ".htm" => "text/html; charset=utf-8",
            ".css" => "text/css; charset=utf-8",
            ".js" => "text/javascript; charset=utf-8",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".svg" => "image/svg+xml",
            ".ico" => "image/x-icon",
            ".json" => "application/json; charset=utf-8",
            _ => "application/octet-stream"
        };
    }
}