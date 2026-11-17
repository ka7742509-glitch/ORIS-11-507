using System.Net;
using MiniHttpServer.Framework.Configuration;
using MiniHttpServer.Framework.Content;
using MiniHttpServer.Framework.Content;

namespace MiniHttpServer.Framework.Handlers;

public sealed class StaticFilesHandler : RequestHandler
{
    private readonly string _staticRoot;

    public StaticFilesHandler()
    {
        var settings = ConfigurationManager.Instance.Settings;
        _staticRoot = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, settings.PublicDirectoryPath));
    }
    
    public override async Task HandleAsync(HttpListenerContext context)
    {
        var settings = ConfigurationManager.Instance.Settings;
        string localPath = context.Request.Url!.LocalPath;

        string? fullPath = StaticFileResolver.Resolve(localPath, settings.BasePath, _staticRoot, settings.DefaultPage);
        if (fullPath is null)
        {
            await HandleNextAsync(context);
            return;
        }

        await using var fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        var response = context.Response;
        response.StatusCode = 200;
        response.ContentType = MimeTypeProvider.GetMimeType(fullPath);
        response.ContentLength64 = fileStream.Length;
        response.Headers["Server"] = "MiniHttpServer/1.0";
        await fileStream.CopyToAsync(response.OutputStream);
    }
}