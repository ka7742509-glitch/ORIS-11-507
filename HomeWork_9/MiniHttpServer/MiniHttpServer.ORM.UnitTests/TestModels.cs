namespace MiniHttpServer.ORM.UnitTests;

public class TestUser
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Age { get; set; }
    public int? Nickname { get; set; }
}

public class TestProduct
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

public enum TestStatus { New = 0, Completed = 1 }

public class TestTask
{
    public int Id { get; set; }
    public TestStatus Status { get; set; }
}