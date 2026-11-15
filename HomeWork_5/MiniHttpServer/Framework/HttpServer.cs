using System.Net;
using MiniHttpServer.Framework.Configuration;
using MiniHttpServer.Framework.Handlers;

namespace MiniHttpServer.Framework;

public class HttpServer
{
    private readonly RequestHandler _firstHandler;
    private HttpListener? _listener;
    private bool _isRunning;

    public HttpServer(RequestHandler firstHandler)
    {
        _firstHandler = firstHandler;
    }

    public void Start()
    {
        if (_isRunning) return;

        _listener = new HttpListener();
        _listener.Prefixes.Add(ConfigurationManager.Instance.Settings.Prefix);
        _listener.Start();
        _isRunning = true;

        Console.WriteLine($"Сервер запущен: {ConfigurationManager.Instance.Settings.Prefix}");
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
            await _firstHandler.HandleAsync(context);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка обработки запроса: {ex.Message}");
            try
            {
                context.Response.StatusCode = 500;
                context.Response.ContentType = "text/plain; charset=utf-8";
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes("500 Internal Server Error");
                context.Response.ContentLength64 = buffer.Length;
                await context.Response.OutputStream.WriteAsync(buffer);
            }
            catch { }
        }
        finally
        {
            context.Response.Close();
        }
    }
}