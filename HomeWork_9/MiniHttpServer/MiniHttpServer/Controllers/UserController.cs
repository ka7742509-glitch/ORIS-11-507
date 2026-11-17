using MiniHttpServer.Framework.Attributes;
using MiniHttpServer.Framework.Configuration;
using MiniHttpServer.Framework.Controllers;
using MiniHttpServer.Framework.Results;
using MiniHttpServer.Models;
using MiniHttpServer.ORM;

namespace MiniHttpServer.Controllers;

[Controller]
public class UserController : BaseController
{
    private static IOrmContext CreateOrm()
        => new OrmContext(ConfigurationManager.Instance.Settings.ConnectionString);

    [Get]
    public async Task<IHttpResult> List()
    {
        IReadOnlyList<User> users = await CreateOrm().GetAllAsync<User>("Users");
        return Page("User/List.html", new { Users = users });
    }

    [Get]
    public async Task<IHttpResult> Products()
    {
        IReadOnlyList<Product> products = await CreateOrm().GetAllAsync<Product>("Products");
        return Json(products);
    }

    [Get]
    public async Task<IHttpResult> Details(int id)
    {
        User? user = await CreateOrm().GetByIdAsync<User>("Users", id);
        return user is null
            ? Json(new { success = false, message = "Пользователь не найден" }, 404)
            : Json(user);
    }

    [Post]
    public async Task<IHttpResult> Create(string name, string email, int age)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email))
            return Json(new { success = false, message = "Имя и email обязательны" }, 400);

        if (age < 0 || age > 150)
            return Json(new { success = false, message = "Некорректный возраст" }, 400);

        int affected = await CreateOrm().InsertAsync("Users", new User { Name = name, Email = email, Age = age });
        return Json(new { success = affected > 0, affected });
    }

    [Post]
    public async Task<IHttpResult> Edit(int id, string name, string email, int age)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email))
            return Json(new { success = false, message = "Имя и email обязательны" }, 400);

        int affected = await CreateOrm().UpdateAsync("Users", id, new User { Name = name, Email = email, Age = age });

        return affected == 0
            ? Json(new { success = false, message = "Запись не найдена" }, 404)
            : Json(new { success = true, affected });
    }

    [Post]
    public async Task<IHttpResult> Remove(int id)
    {
        int affected = await CreateOrm().DeleteAsync("Users", id);

        return affected == 0
            ? Json(new { success = false, message = "Запись не найдена" }, 404)
            : Json(new { success = true, affected });
    }

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
}