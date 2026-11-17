namespace MiniHttpServer.Services;

public interface IEmailService
{
    Task<bool> SendAsync(string demoUser, string message);
}