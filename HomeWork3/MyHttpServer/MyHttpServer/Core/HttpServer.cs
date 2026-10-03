using System.Net;
using System.Text;
using MyHttpServer.Services;
using MyHttpServer.Settings;

namespace MyHttpServer.Core;
public class HttpServer
{
    private readonly ContentTypeService _contentTypeService;
    private readonly StaticFileService _staticFileService;
    private HttpListener? _listener;
    private bool _isRunning;

    public HttpServer()
    {
        _contentTypeService = new ContentTypeService();
        _staticFileService = new StaticFileService();
    }

    public void Start()
    {
        if (_isRunning) return;

        _listener = new HttpListener();
        _listener.Prefixes.Add(ServerSettings.Instance.Prefix);
        _listener.Start();
        _isRunning = true;

        Console.WriteLine($"Сервер запущен на {ServerSettings.Instance.Prefix}");
        Receive();
    }

    public void Stop()
    {
        if (!_isRunning) return;

        _isRunning = false;
        _listener?.Stop();
        _listener?.Close();
        _listener = null;

        Console.WriteLine("Сервер остановлен.");
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
        catch (Exception)
        {
            return;
        }

        Receive();

        try
        {
            await ProcessRequestAsync(context);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка обработки запроса: {ex.Message}");
        }
    }

    private async Task ProcessRequestAsync(HttpListenerContext context)
    {
        var request = context.Request;
        var response = context.Response;

        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {request.HttpMethod} {request.Url?.LocalPath}");

        string? filePath = _staticFileService.ResolveFilePath(request.Url!.LocalPath);

        if (filePath == null)
        {
            response.StatusCode = (int)HttpStatusCode.NotFound;
            filePath = _staticFileService.GetNotFoundPagePath();

            if (filePath == null)
            {
                byte[] errorBytes = Encoding.UTF8.GetBytes("404 Not Found");
                response.ContentType = "text/plain; charset=utf-8";
                response.ContentLength64 = errorBytes.Length;
                using var errorOutput = response.OutputStream;
                await errorOutput.WriteAsync(errorBytes);
                await errorOutput.FlushAsync();
                return;
            }
        }

        string extension = Path.GetExtension(filePath);
        byte[] buffer = await _staticFileService.ReadFileAsync(filePath);

        response.ContentType = _contentTypeService.GetContentType(extension);
        response.ContentLength64 = buffer.Length;

        using var output = response.OutputStream;
        await output.WriteAsync(buffer);
        await output.FlushAsync();

        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Ответ отправлен: {extension}");
    }
}