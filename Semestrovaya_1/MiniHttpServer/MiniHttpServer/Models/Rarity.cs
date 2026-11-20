namespace MiniHttpServer.Models;

public class Rarity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Color { get; set; }
}