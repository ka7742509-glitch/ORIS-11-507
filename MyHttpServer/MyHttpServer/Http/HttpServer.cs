using System.Net;
using MyHttpServer.Framework.Handlers;

namespace MyHttpServer.Http;

public class HttpServer
{
    private int _port = 8888;
    private string _staticFolderPath = "static";
    private Handler? _handlerChain;
    private HttpListener _listener;

    public void Start()
    {
        var staticHandler = new StaticFileHandler(_staticFolderPath);
        var controllerHandler = new ControllerHandler();
        var notFoundHandler = new NotFoundHandler(Path.Combine(_staticFolderPath, "404.html"));

        staticHandler.SetNext(controllerHandler).SetNext(notFoundHandler);
        _handlerChain = staticHandler;

        _listener = new HttpListener();
        _listener.Prefixes.Add("http://127.0.0.1:" + _port.ToString() + "/");
        _listener.Start();
        Console.WriteLine("Сервер начал свою работу");
        Receive();
    }

    public void Stop()
    {
        _listener.Stop();
        Console.WriteLine("Сервер завершил свою работу");
    }

    private void Receive()
    {
        _listener.BeginGetContext(new AsyncCallback(ListenerCallback), _listener);
    }

    private async void ListenerCallback(IAsyncResult result)
    {
        if (_listener.IsListening)
        {
            var context = _listener.EndGetContext(result);
            Console.WriteLine("Пришел запрос");

            try
            {
                if (_handlerChain != null)
                    await _handlerChain.HandleRequestAsync(context);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Критическая ошибка: {ex.Message}");
                var response = context.Response;
                response.StatusCode = 500;
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes("500 Internal Server Error");
                response.ContentType = "text/plain; charset=utf-8";
                response.ContentLength64 = buffer.Length;
                using var output = response.OutputStream;
                await output.WriteAsync(buffer);
            }
            finally
            {
                context.Response.Close();
            }

            Console.WriteLine("Запрос обработан");
            Receive();
        }
    }
}