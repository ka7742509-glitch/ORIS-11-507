using MyHttpServer.Framework.Attributes;

namespace MyHttpServer.Controllers;

[Controller("auth")]
public class AuthController
{
    [Get("login")]
    public void LoginGet()
    {
        Console.WriteLine("GET /auth/login - страница входа");
    }

    [Post("login")]
    public void LoginPost(string email, string password)
    {
        Console.WriteLine($"POST /auth/login");
        Console.WriteLine($"Email: {email}");
        Console.WriteLine($"Password: {password}");
    }
}