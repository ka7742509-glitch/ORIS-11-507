namespace MiniHttpServer.Models;

public class CardSet
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime ReleaseDate { get; set; }
    public int CardCount { get; set; }
}