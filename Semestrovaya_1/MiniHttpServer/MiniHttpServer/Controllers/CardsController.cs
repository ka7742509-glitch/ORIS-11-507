using MiniHttpServer.Framework.Attributes;
using MiniHttpServer.Framework.Controllers;
using MiniHttpServer.Framework.Results;
using MiniHttpServer.Models;
using MiniHttpServer.Repositories;

namespace MiniHttpServer.Controllers;

[Controller]
public class CardsController : BaseController
{
    private const int DefaultPageSize = 72;
    private readonly ICardRepository _repository;

    public CardsController()
    {
        _repository = new CardRepository();
    }
    
        [Get]
    public async Task<IHttpResult> List(int page = 1, string sort = "number", string order = "asc", string q = "")
    {
        if (page < 1) page = 1;

        var (cards, totalCount) = await _repository.GetPagedAsync(page, DefaultPageSize, sort, order, q);

        int totalPages = (totalCount + DefaultPageSize - 1) / DefaultPageSize;
        if (totalPages == 0) totalPages = 1;
        if (page > totalPages) page = totalPages;

        int from = (page - 1) * DefaultPageSize + 1;
        int to = Math.Min(page * DefaultPageSize, totalCount);

        var cardViewModels = cards.Select(c => new
        {
            c.Id,
            c.Name,
            NumberPadded = c.Number.ToString("D3"),
            c.TypeLine,
            c.ManaCost,
            c.ImageUrl
        }).ToList();
        
        var pageLinks = new List<object>();
        if (totalPages > 1)
        {
            string Url(int p) => "/cards/list?page=" + p + "&sort=" + sort + "&order=" + order + "&q=" + Uri.EscapeDataString(q);

            pageLinks.Add(page > 1
                ? new { Url = Url(1), Label = "|«", IsActive = false, Disabled = false }
                : new { Url = "", Label = "|«", IsActive = false, Disabled = true });

            if (page > 1)
                pageLinks.Add(new { Url = Url(page - 1), Label = "‹", IsActive = false, Disabled = false });

            int start = Math.Min(Math.Max(1, page - 1), Math.Max(1, totalPages - 3));
            int end = Math.Min(start + 3, totalPages);
            for (int i = start; i <= end; i++)
                pageLinks.Add(new { Url = Url(i), Label = i.ToString(), IsActive = i == page, Disabled = false });

            if (page < totalPages)
                pageLinks.Add(new { Url = Url(page + 1), Label = "›", IsActive = false, Disabled = false });

            pageLinks.Add(page < totalPages
                ? new { Url = Url(totalPages), Label = "»|", IsActive = false, Disabled = false }
                : new { Url = "", Label = "»|", IsActive = false, Disabled = true });
        }
        
        return Page("Cards/List.html", new
        {
            Set = new CardSet
            {
                Code = "HOB",
                Name = "Magic: The Gathering | The Hobbit",
                ReleaseDate = new DateTime(2026, 7, 31),
                CardCount = totalCount
            },
            Cards = cardViewModels,
            Page = page,
            TotalPages = totalPages,
            TotalCount = totalCount,
            From = from,
            To = to,
            Sort = sort,
            Order = order,
            Search = q,
            PageLinks = pageLinks
        });
    }
    
    [Get]
    public async Task<IHttpResult> Details(int id)
    {
        var card = await _repository.GetByIdAsync(id);
        if (card is null)
            return new JsonResult(new { success = false, message = "Карта не найдена" }, 404);

        var set = (await _repository.GetSetByIdAsync(card.SetId))
                  ?? new CardSet { Code = "HOB", Name = "Magic: The Gathering | The Hobbit" };

        var rarity = (await _repository.GetRarityByIdAsync(card.RarityId))
                     ?? new Rarity { Name = "Unknown" };

        var artist = (await _repository.GetArtistByIdAsync(card.ArtistId))
                     ?? new Artist { Name = "Unknown" };

        var languages = new[]
        {
            new { Code = "DE", Name = "German", IsActive = false },
            new { Code = "EN", Name = "English", IsActive = true },
            new { Code = "SP", Name = "Spanish", IsActive = false },
            new { Code = "FR", Name = "French", IsActive = false },
            new { Code = "IT", Name = "Italian", IsActive = false },
            new { Code = "JP", Name = "Japanese", IsActive = false }
        };
        return Page("Cards/Details.html", new
        {
            Card = card,
            Set = set,
            Rarity = rarity,
            Artist = artist,
            NumberPadded = card.Number.ToString("D3"),
            PowerToughness = !string.IsNullOrEmpty(card.Power) && !string.IsNullOrEmpty(card.Toughness)
                ? $"{card.Power}/{card.Toughness}"
                : null,
            Languages = languages,
            LegalFormats = "Brawl, Commander, Duel, Legacy, Modern, MTGO, Penny, Premodern, Standard"
        });
    }
    
    [Get]
    public async Task<IHttpResult> Search(string q = "")
    {
        if (string.IsNullOrWhiteSpace(q))
            return Json(Array.Empty<object>());

        var results = await _repository.SearchAsync(q, 10);
        return Json(results.Select(c => new { id = c.Id, name = c.Name, number = c.Number }).ToList());
    }
}