namespace MiniHttpServer.Models;

public class Card
{
    public int Id { get; set; }
    public int SetId { get; set; }
    public int ArtistId { get; set; }
    public int RarityId { get; set; }
    public int Number { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ManaCost { get; set; }
    public string TypeLine { get; set; } = string.Empty;
    public string? RulesText { get; set; }
    public string? FlavorText { get; set; }
    public string? Power { get; set; }
    public string? Toughness { get; set; }
    public string Language { get; set; } = "English";
    public string? ImageUrl { get; set; }
}