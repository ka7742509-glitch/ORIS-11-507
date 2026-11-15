using System.Net;
using System.Text;

namespace HttpServer
{
    public class HttpServer
    {
        private readonly HttpListener _listener;
        private readonly string _htmlFilePath;
        private CancellationTokenSource? _cts;
        private Task? _listenTask;
    
        public HttpServer(string address, string htmlFilePath)
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add(address);
            _htmlFilePath = htmlFilePath;
        }
    
        public void Start()
        {
            _cts = new CancellationTokenSource();
            _listener.Start();
            Console.WriteLine($"Сервер запущен на {_listener.Prefixes.First()}");
            Console.WriteLine("Для остановки введите 'stop' в консоль.");
    
            _listenTask = Task.Run(() => ListenLoop(_cts.Token));
        }
    
        private async Task ListenLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var context = await _listener.GetContextAsync();
                    
                    _ = Task.Run(() => HandleRequest(context), token);
                }
                catch (HttpListenerException) when (token.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при обработке запроса: {ex.Message}");
                }
            }
        }
    
        private async Task HandleRequest(HttpListenerContext context)
        {
            var response = context.Response;
            
            try
            {
                string htmlContent = await File.ReadAllTextAsync(_htmlFilePath);
                byte[] buffer = Encoding.UTF8.GetBytes(htmlContent);
    
                response.ContentType = "text/html; charset=utf-8";
                response.ContentLength64 = buffer.Length;
                
                using var output = response.OutputStream;
                await output.WriteAsync(buffer);
                await output.FlushAsync();
    
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Запрос обработан: {context.Request.Url?.AbsolutePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при отправке ответа: {ex.Message}");
                
                byte[] errorBuffer = Encoding.UTF8.GetBytes("<h1>500 Internal Server Error</h1>");
                response.StatusCode = 500;
                response.ContentLength64 = errorBuffer.Length;
                using var output = response.OutputStream;
                await output.WriteAsync(errorBuffer);
            }
        }
    
        public void Stop()
        {
            Console.WriteLine("Остановка сервера...");
            _cts?.Cancel();
            _listener.Stop();
            _listener.Close();
            Console.WriteLine("Сервер остановлен.");
        }
    }
}
