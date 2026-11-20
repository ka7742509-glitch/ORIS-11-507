using System.Net;
using System.Text;
using MiniHttpServer.Framework.Configuration;
using MiniHttpServer.Framework.Handlers;

namespace MiniHttpServer.Framework;

public class HttpServer
{
    private readonly RequestHandler _firstHandler;
    private HttpListener? _listener;
    private volatile bool _isRunning;

    public HttpServer(RequestHandler firstHandler)
    {
        _firstHandler = firstHandler ?? throw new ArgumentNullException(nameof(firstHandler));
    }

    public void Start()
    {
        if (_isRunning) return;

        try
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add(ConfigurationManager.Instance.Settings.Prefix);
            _listener.Start();
            _isRunning = true;
            Console.WriteLine($"Сервер запущен: {ConfigurationManager.Instance.Settings.Prefix}");
            Receive();
        }
        catch (Exception ex)
        {
            _isRunning = false;
            Console.WriteLine($"Не удалось запустить сервер: {ex.Message}");
            Console.WriteLine("Проверьте права: netsh http add urlacl url=http://localhost:8888/ user=%USERDOMAIN%\\%USERNAME%");
        }
    }

    public void Stop()
    {
        if (!_isRunning) return;
        _isRunning = false;
        try { _listener?.Stop(); } catch { }
        try { _listener?.Close(); } catch { }
        Console.WriteLine("Сервер остановлен");
    }

    private void Receive()
    {
        if (!_isRunning || _listener is null) return;

        try
        {
            _listener.BeginGetContext(ListenerCallback, _listener);
        }
        catch (Exception ex)
        {
            if (_isRunning)
                Console.WriteLine($"Ошибка приёма запроса: {ex.Message}");
        }
    }

    private async void ListenerCallback(IAsyncResult result)
    {
        HttpListenerContext? context = null;

        try
        {
            if (_listener is null) return;

            try
            {
                context = _listener.EndGetContext(result);
            }
            catch
            {
                return;
            }

            Receive();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка цикла приёма: {ex.Message}");
            return;
        }

        try
        {
            await _firstHandler.HandleAsync(context);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] {context.Request.HttpMethod} {context.Request.Url?.LocalPath}");
            Console.WriteLine($"        {ex.GetType().Name}: {ex.Message}");
            await TrySendErrorAsync(context, 500, "Внутренняя ошибка сервера");
        }
        finally
        {
            SafeClose(context);
        }
    }

    private static async Task TrySendErrorAsync(HttpListenerContext context, int code, string message)
    {
        try
        {
            string home = ConfigurationManager.Instance.Settings.HomePagePath;
            byte[] buffer = Encoding.UTF8.GetBytes(
                "<!DOCTYPE html><html lang=\"ru\"><head><meta charset=\"utf-8\">" +
                $"<title>{code}</title></head><body><h1>{code}</h1><p>{message}</p>" +
                $"<p><a href=\"{home}\">Вернуться на главную</a></p></body></html>");

            context.Response.StatusCode = code;
            context.Response.ContentType = "text/html; charset=utf-8";
            context.Response.ContentLength64 = buffer.Length;
            await context.Response.OutputStream.WriteAsync(buffer);
        }
        catch
        {
        }
    }

    private static void SafeClose(HttpListenerContext context)
    {
        try
        {
            context.Response.Close();
        }
        catch
        {
        }
    }
}