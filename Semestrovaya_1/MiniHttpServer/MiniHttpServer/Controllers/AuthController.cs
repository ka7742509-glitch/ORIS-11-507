using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using MiniHttpServer.Framework.Attributes;
using MiniHttpServer.Framework.Configuration;
using MiniHttpServer.Framework.Controllers;
using MiniHttpServer.Framework.Results;

namespace MiniHttpServer.Controllers;

[Controller]
public class AuthController : BaseController
{
    private const string SessionCookieName = "sessionId";
    private static readonly TimeSpan SessionLifetime = TimeSpan.FromMinutes(30);
    private static readonly Regex LoginPattern = new(@"^[a-zA-Z0-9_]{3,20}$", RegexOptions.Compiled);

    [Get]
    public IHttpResult Login()
    {
        if (IsAuthenticated())
            return Redirect("/admin/index");

        return Page("Admin/Login.html", new
        {
            Error = string.Empty,
            Login = string.Empty
        });
    }

    [Post]
    public IHttpResult Login(string login = "", string password = "")
    {
        var settings = ConfigurationManager.Instance.Settings;
        login = login.Trim();

        string error = Validate(login, password, settings);
        if (error.Length > 0)
            return Page("Admin/Login.html", new { Error = error, Login = login }, 400);

        string computedHash = ComputeHash(password);

        bool loginMatches = string.Equals(login, settings.AdminLogin, StringComparison.OrdinalIgnoreCase);
        bool passwordMatches = CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(computedHash),
            Encoding.UTF8.GetBytes(settings.AdminPasswordHash.Trim().ToUpperInvariant()));

        if (!loginMatches || !passwordMatches)
            return Page("Admin/Login.html", new { Error = "Неверный логин или пароль.", Login = login }, 401);

        string sessionId = SessionService.CreateSession(1);

        var cookie = new Cookie(SessionCookieName, sessionId, "/")
        {
            HttpOnly = true,
            Secure = Context.Request.IsSecureConnection,
            Expires = DateTime.UtcNow.Add(SessionLifetime)
        };
        Context.Response.SetCookie(cookie);

        return Redirect("/admin/index");
    }

    [Post]
    public IHttpResult Logout()
    {
        string? sessionId = Context.Request.Cookies[SessionCookieName]?.Value;
        SessionService.RemoveSession(sessionId);

        var cookie = new Cookie(SessionCookieName, string.Empty, "/")
        {
            HttpOnly = true,
            Secure = Context.Request.IsSecureConnection,
            Expires = DateTime.UtcNow.AddDays(-1)
        };
        Context.Response.SetCookie(cookie);

        return Redirect("/auth/login");
    }

    private bool IsAuthenticated()
    {
        string? sessionId = Context.Request.Cookies[SessionCookieName]?.Value;
        return SessionService.GetUserId(sessionId).HasValue;
    }

    private static string Validate(string login, string password, ServerSettings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.AdminPasswordHash))
            return "Панель управления не настроена: задайте AdminPasswordHash в settings.json.";

        if (login.Length == 0)
            return "Введите логин.";

        if (!LoginPattern.IsMatch(login))
            return "Логин: от 3 до 20 символов, только латинские буквы, цифры и подчёркивание.";

        if (password.Length == 0)
            return "Введите пароль.";

        if (password.Length > 64)
            return "Пароль не должен превышать 64 символа.";

        return string.Empty;
    }

    private static string ComputeHash(string value)
    {
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(hash);
    }
}