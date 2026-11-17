using System.Net;
using MiniHttpServer.Framework.Attributes;
using MiniHttpServer.Framework.Controllers;
using MiniHttpServer.Framework.Results;
using MiniHttpServer.Services;

namespace MiniHttpServer.Controllers;

[Controller]
public class AuthController : BaseController
{
    private readonly IEmailService _emailService;

    public AuthController()
    {
        _emailService = new EmailService();
    }

    [Post]
    public async Task<IHttpResult> SendEmail(string demoUser, string message)
    {
        if (string.IsNullOrWhiteSpace(demoUser))
            return Json(new { success = false, message = "Поле demoUser обязательно для заполнения." }, 400);

        if (demoUser.Length > 50)
            return Json(new { success = false, message = "Поле demoUser не должно превышать 50 символов." }, 400);

        if (string.IsNullOrWhiteSpace(message))
            return Json(new { success = false, message = "Поле message обязательно для заполнения." }, 400);

        if (message.Length > 500)
            return Json(new { success = false, message = "Поле message не должно превышать 500 символов." }, 400);

        string safeUser = WebUtility.HtmlEncode(demoUser);
        string safeMessage = WebUtility.HtmlEncode(message);

        bool sent = await _emailService.SendAsync(safeUser, safeMessage);

        if (!sent)
            return Json(new { success = false, message = "Не удалось отправить сообщение" }, 502);

        return Json(new { success = true, message = "Сообщение отправлено" });
    }
}