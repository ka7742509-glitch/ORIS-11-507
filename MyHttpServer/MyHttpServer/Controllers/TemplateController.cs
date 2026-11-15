using MiniTemplateEngine;
using MyHttpServer.Framework.Attributes;

namespace MyHttpServer.Controllers;

[Controller("template")]
public class TemplateController
{
    private readonly IHtmlTemplateRenderer _renderer;

    public TemplateController()
    {
        _renderer = new HtmlTemplateRenderer();
    }
    
    [Get("render")]
    public void RenderDemo()
    {
        var user = new User
        {
            Name = "Алиса",
            Age = 20,
            IsActive = true,
            Books = new List<Book>
            {
                new Book { Title = "Война и мир", Author = "Толстой" },
                new Book { Title = "Преступление и наказание", Author = "Достоевский" }
            }
        };
    
        var wrapper = new { user = user }; 

        string template = @"
<html>
<body>
    <h1>Профиль пользователя</h1>
    <p>Имя: ${user.Name}</p>
    <p>Возраст: ${user.Age}</p>
    
    $if(user.Age >= 18)
        <p style='color:green'>Совершеннолетний</p>
    $else
        <p style='color:red'>Несовершеннолетний</p>
    $endif

    <h2>Книги:</h2>
    <ul>
    $foreach(var book in user.Books)
        <li>${book.Title} — ${book.Author}</li>
    $endfor
    </ul>
</body>
</html>";

        string result = _renderer.RenderFromString(template, wrapper);

        string outputPath = Path.Combine(AppContext.BaseDirectory, "static", "generated_profile.html");
        File.WriteAllText(outputPath, result);

        Console.WriteLine($"[Template] Сгенерирован файл: {outputPath}");
    }
}

public class User
{
    public string Name { get; set; } = "";
    public int Age { get; set; }
    public bool IsActive { get; set; }
    public List<Book> Books { get; set; } = new();
}

public class Book
{
    public string Title { get; set; } = "";
    public string Author { get; set; } = "";
}