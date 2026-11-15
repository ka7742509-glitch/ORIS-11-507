using System.Net;
using System.Text;

namespace MiniHttpServer
{
	public class HttpServer
    {
        private HttpListener? _listener;
        private bool _isRunning;
        private readonly string _staticRoot;
    
        public HttpServer()
        {
            var settings = ConfigurationManager.Instance.Settings;
            _staticRoot = Path.GetFullPath(
                Path.Combine(AppContext.BaseDirectory, settings.PublicDirectoryPath));
        }
    
        public void Start()
        {
            if (_isRunning) return;
    
            _listener = new HttpListener();
            _listener.Prefixes.Add(ConfigurationManager.Instance.Settings.Prefix);
            _listener.Start();
            _isRunning = true;
    
            Console.WriteLine($"Сервер запущен: {ConfigurationManager.Instance.Settings.Prefix}");
            Console.WriteLine($"Каталог статики: {_staticRoot}");
            Receive();
        }
    
        public void Stop()
        {
            if (!_isRunning) return;
            _isRunning = false;
            _listener?.Stop();
            _listener?.Close();
            Console.WriteLine("Сервер остановлен");
        }
    
        private void Receive()
        {
            if (!_isRunning || _listener == null) return;
            _listener.BeginGetContext(ListenerCallback, _listener);
        }
    
        private async void ListenerCallback(IAsyncResult result)
        {
            if (!_isRunning || _listener == null) return;
    
            HttpListenerContext context;
            try
            {
                context = _listener.EndGetContext(result);
            }
            catch
            {
                return;
            }
    
            Receive();
    
            try
            {
                await SendStaticFileAsync(context);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка обработки: {ex.Message}");
                try { await SendStatusAsync(context.Response, 500, "Internal Server Error"); }
                catch { }
            }
            finally
            {
                context.Response.Close();
            }
        }
    
        private async Task SendStaticFileAsync(HttpListenerContext context)
        {
            var settings = ConfigurationManager.Instance.Settings;
            var response = context.Response;
    
            string localPath = context.Request.Url!.LocalPath;
    
            if (!localPath.StartsWith(settings.BasePath))
            {
                await SendStatusAsync(response, 404, "Not Found");
                return;
            }
    
            string relativePath = localPath.Substring(settings.BasePath.Length).Replace('/', Path.DirectorySeparatorChar);
            string fullPath = Path.GetFullPath(Path.Combine(_staticRoot, relativePath));
    
            if (!fullPath.StartsWith(_staticRoot, StringComparison.OrdinalIgnoreCase))
            {
                await SendStatusAsync(response, 404, "Not Found");
                return;
            }
    
            if (relativePath.Length == 0 || localPath.EndsWith("/"))
                fullPath = Path.Combine(fullPath, settings.DefaultPage);
            else if (!Path.HasExtension(fullPath))
            {
                await SendStatusAsync(response, 404, "Not Found");
                return;
            }
    
            if (!File.Exists(fullPath))
            {
                await SendStatusAsync(response, 404, "Not Found");
                return;
            }
    
            await using var fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
    
            response.StatusCode = 200;
            response.ContentType = GetContentType(fullPath);
            response.ContentLength64 = fileStream.Length;
            response.Headers["Server"] = "MiniHttpServer/1.0";
    
            await fileStream.CopyToAsync(response.OutputStream);
    
            Console.WriteLine($"[200] {localPath} ({GetContentType(fullPath)}, {fileStream.Length} байт)");
        }
    
        private static string GetContentType(string filePath)
        {
            return Path.GetExtension(filePath).ToLowerInvariant() switch
            {
                ".html" or ".htm" => "text/html; charset=utf-8",
                ".css" => "text/css; charset=utf-8",
                ".js" => "text/javascript; charset=utf-8",
                ".json" => "application/json; charset=utf-8",
                ".png" => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".svg" => "image/svg+xml",
                ".gif" => "image/gif",
                ".ico" => "image/x-icon",
                ".webp" => "image/webp",
                ".woff" => "font/woff",
                ".woff2" => "font/woff2",
                ".ttf" => "font/ttf",
                ".pdf" => "application/pdf",
                ".txt" => "text/plain; charset=utf-8",
                _ => "application/octet-stream"
            };
        }
    
        private static async Task SendStatusAsync(HttpListenerResponse response, int code, string message)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(
                $"<!DOCTYPE html><html lang=\"ru\"><head><meta charset=\"utf-8\"><title>{code}</title></head>" +
                $"<body><h1>{code} {message}</h1></body></html>");
    
            response.StatusCode = code;
            response.ContentType = "text/html; charset=utf-8";
            response.ContentLength64 = buffer.Length;
            response.Headers["Server"] = "MiniHttpServer/1.0";
    
            await response.OutputStream.WriteAsync(buffer);
            Console.WriteLine($"[{code}] {message}");
        }
    }
}
