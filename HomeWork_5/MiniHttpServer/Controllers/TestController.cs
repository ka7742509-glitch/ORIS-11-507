using MiniHttpServer.Framework.Attributes;

namespace MiniHttpServer.Controllers;

[Controller]
public class TestController
{
    [Get]
    public string Index()
    {
        return "Hello from controller!";
    }

    [Get]
    public string Hello()
    {
        return "Hello World!";
    }

    [Post]
    public string Submit()
    {
        return "POST request processed";
    }

    [Get]
    public string Details(int id, string name)
    {
        return $"ID: {id}, Name: {name}";
    }

    [Post]
    public string Echo(string demoUser, string message)
    {
        return $"User: {demoUser}, Message: {message}";
    }
}