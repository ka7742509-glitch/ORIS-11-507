namespace MyHttpServer.Services;

public class ContentTypeService
{
    public string GetContentType(string extension)
    {
        switch (extension.ToLower())
        {
            case ".html":
            case ".htm":
                return "text/html; charset=utf-8";
            case ".css":
                return "text/css; charset=utf-8";
            case ".js":
                return "text/javascript; charset=utf-8";
            case ".jpg":
            case ".jpeg":
                return "image/jpeg";
            case ".png":
                return "image/png";
            case ".svg":
                return "image/svg+xml";
            case ".ico":
                return "image/x-icon";
            case ".json":
                return "application/json; charset=utf-8";
            case ".txt":
                return "text/plain; charset=utf-8";
            default:
                return "application/octet-stream";
        }
    }
}