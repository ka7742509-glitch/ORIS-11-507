namespace MiniHttpServer.Models;

public class Artist
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int CardCount { get; set; }
    public string? Url { get; set; }
}