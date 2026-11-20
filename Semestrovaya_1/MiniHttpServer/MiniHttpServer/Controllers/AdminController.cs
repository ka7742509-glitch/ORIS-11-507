using MiniHttpServer.Framework.Attributes;
using MiniHttpServer.Framework.Controllers;
using MiniHttpServer.Framework.Results;
using MiniHttpServer.Models;
using MiniHttpServer.Repositories;
using MiniHttpServer.Validation;
using Npgsql;

namespace MiniHttpServer.Controllers;

[Controller]
[Authorize]
public class AdminController : BaseController
{
    private readonly ICardRepository _repository;

    public AdminController()
    {
        _repository = new CardRepository();
    }
    
    [Get]
    public async Task<IHttpResult> Index()
    {
        var cards = await _repository.GetAllCardsAsync();
        return Page("Admin/Dashboard.html", new { Cards = cards });
    }

    [Get]
    public async Task<IHttpResult> Create()
    {
        var rarities = await _repository.GetAllRaritiesAsync();
        var artists = await _repository.GetAllArtistsAsync();
        return Page("Admin/Create.html", new 
        { 
            Rarities = rarities, 
            Artists = artists,
            Error = string.Empty,
            Card = new Card() 
        });
    }
    
    [Post]
    public async Task<IHttpResult> Create(
        string name, 
        string? manaCost, 
        string typeLine, 
        int rarityId, 
        int artistId,
        string? rulesText, 
        string? flavorText, 
        string? power, 
        string? toughness, 
        string? imageUrl)
    {
        var rarities = await _repository.GetAllRaritiesAsync();
        var artists = await _repository.GetAllArtistsAsync();

        var card = new Card
        {
            Name = name, 
            ManaCost = manaCost, 
            TypeLine = typeLine,
            RarityId = rarityId, 
            ArtistId = artistId,
            RulesText = rulesText, 
            FlavorText = flavorText,
            Power = power, 
            Toughness = toughness, 
            ImageUrl = imageUrl,
            SetId = 1,
            Number = await _repository.GetNextNumberAsync(1),
            Language = "English"
        };

        var nameResult = CardValidator.ValidateName(name);
        if (!nameResult.IsValid)
            return Page("Admin/Create.html", new { Rarities = rarities, Artists = artists, Error = nameResult.ErrorMessage, Card = card });

        var manaResult = CardValidator.ValidateManaCost(manaCost);
        if (!manaResult.IsValid)
            return Page("Admin/Create.html", new { Rarities = rarities, Artists = artists, Error = manaResult.ErrorMessage, Card = card });

        await _repository.InsertAsync(card);
        return Redirect("/admin/index");
    }

    [Post]
    public async Task<IHttpResult> Edit(
        int id, 
        string name, 
        string? manaCost, 
        string typeLine, 
        int rarityId, 
        int artistId,
        string? rulesText, 
        string? flavorText, 
        string? power, 
        string? toughness, 
        string? imageUrl)
    {
        var card = await _repository.GetByIdAsync(id);
        if (card is null) return Redirect("/admin/index");

        var rarities = await _repository.GetAllRaritiesAsync();
        var artists = await _repository.GetAllArtistsAsync();

        card.Name = name; 
        card.ManaCost = manaCost; 
        card.TypeLine = typeLine;
        card.RarityId = rarityId; 
        card.ArtistId = artistId;
        card.RulesText = rulesText; 
        card.FlavorText = flavorText;
        card.Power = power; 
        card.Toughness = toughness; 
        card.ImageUrl = imageUrl;

        var nameResult = CardValidator.ValidateName(name);
        if (!nameResult.IsValid)
            return Page("Admin/Edit.html", new { Card = card, Rarities = rarities, Artists = artists, Error = nameResult.ErrorMessage });

        var manaResult = CardValidator.ValidateManaCost(manaCost);
        if (!manaResult.IsValid)
            return Page("Admin/Edit.html", new { Card = card, Rarities = rarities, Artists = artists, Error = manaResult.ErrorMessage });

        await _repository.UpdateAsync(card);
        return Redirect("/admin/index");
    }

    [Get]
    public async Task<IHttpResult> Edit(int id)
    {
        var card = await _repository.GetByIdAsync(id);
        if (card is null) return Redirect("/admin/index");

        var rarities = await _repository.GetAllRaritiesAsync();
        var artists = await _repository.GetAllArtistsAsync();

        return Page("Admin/Edit.html", new 
        { 
            Card = card, Rarities = rarities, Artists = artists, Error = string.Empty 
        });
    }
    
    [Post]
    public async Task<IHttpResult> Delete(int id)
    {
        await _repository.DeleteAsync(id);
        return Redirect("/admin/index");
    }
}