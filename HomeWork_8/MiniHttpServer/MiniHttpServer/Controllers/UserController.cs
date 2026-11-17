using MiniHttpServer.Framework.Attributes;
using MiniHttpServer.Framework.Controllers;
using MiniHttpServer.Framework.Results;

namespace MiniHttpServer.Controllers;

[Controller]
public class UserController : BaseController
{
    [Get]
    public IHttpResult Profile()
    {
        var user = new
        {
            Name = "Alex",
            Age = 20,
            Email = "alex@example.com",
            IsActive = true,
            Books = new[]
            {
                new { Title = "Война и мир", Author = "Толстой" },
                new { Title = "Преступление и наказание", Author = "Достоевский" }
            }
        };

        return Page("User/Profile.html", user);
    }

    [Get]
    public IHttpResult List()
    {
        var model = new
        {
            Users = new[]
            {
                new { Name = "Alex", IsActive = true },
                new { Name = "Bob", IsActive = false },
                new { Name = "Carol", IsActive = true }
            }
        };

        return Page("User/List.html", model);
    }
}