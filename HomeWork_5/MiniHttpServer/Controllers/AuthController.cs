using MiniHttpServer.Framework.Attributes;
using MiniHttpServer.Services;

namespace MiniHttpServer.Controllers;

[Controller]
public class AuthController
{
    private readonly IEmailService _emailService;

    public AuthController()
    {
        _emailService = new EmailService();
    }

    [Post]
    public async Task<string> SendEmail(string demoUser, string message)
    {
        if (string.IsNullOrWhiteSpace(demoUser))
            throw new FormatException("Поле demoUser обязательно для заполнения.");

        if (demoUser.Length > 50)
            throw new FormatException("Поле demoUser не должно превышать 50 символов.");

        if (string.IsNullOrWhiteSpace(message))
            throw new FormatException("Поле message обязательно для заполнения.");

        if (message.Length > 500)
            throw new FormatException("Поле message не должно превышать 500 символов.");

        bool sent = await _emailService.SendAsync(demoUser, message);

        return sent
            ? $"Письмо отправлено. Пользователь: {demoUser}"
            : "Не удалось отправить письмо. Подробности в консоли сервера.";
    }
}