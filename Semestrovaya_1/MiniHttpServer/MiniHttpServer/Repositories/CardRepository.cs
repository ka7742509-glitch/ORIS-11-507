using MiniHttpServer.Framework.Configuration;
using MiniHttpServer.Models;
using MiniHttpServer.ORM;

namespace MiniHttpServer.Repositories;

public sealed class CardRepository : ICardRepository
{
    private readonly OrmContext _orm;

    public CardRepository()
    {
        _orm = new OrmContext(ConfigurationManager.Instance.Settings.ConnectionString);
    }

    public async Task<CardSet?> GetSetByIdAsync(int id)
    {
        return await _orm.GetByIdAsync<CardSet>("CardSets", id);
    }

    public async Task<Rarity?> GetRarityByIdAsync(int id)
    {
        return await _orm.GetByIdAsync<Rarity>("Rarities", id);
    }

    public async Task<Artist?> GetArtistByIdAsync(int id)
    {
        return await _orm.GetByIdAsync<Artist>("Artists", id);
    }

    public async Task<(IReadOnlyList<Card> Cards, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? sortBy = null, string? sortOrder = null, string? search = null)
    {
        var allCards = await _orm.GetAllAsync<Card>("Cards");

        // Поиск
        if (!string.IsNullOrWhiteSpace(search))
        {
            string searchLower = search.ToLowerInvariant();
            allCards = allCards.Where(c =>
                c.Name.ToLowerInvariant().Contains(searchLower) ||
                (c.TypeLine?.ToLowerInvariant().Contains(searchLower) ?? false) ||
                (c.RulesText?.ToLowerInvariant().Contains(searchLower) ?? false)).ToList();
        }

        // Сортировка
        bool desc = sortOrder?.ToLowerInvariant() == "desc";
        allCards = sortBy?.ToLowerInvariant() switch
        {
            "name" => desc
                ? allCards.OrderByDescending(c => c.Name).ToList()
                : allCards.OrderBy(c => c.Name).ToList(),
            "set" => desc
                ? allCards.OrderByDescending(c => c.SetId).ToList()
                : allCards.OrderBy(c => c.SetId).ToList(),
            "rarity" => desc
                ? allCards.OrderByDescending(c => c.RarityId).ToList()
                : allCards.OrderBy(c => c.RarityId).ToList(),
            "mana" => desc
                ? allCards.OrderByDescending(c => c.ManaCost).ToList()
                : allCards.OrderBy(c => c.ManaCost).ToList(),
            "type" => desc
                ? allCards.OrderByDescending(c => c.TypeLine).ToList()
                : allCards.OrderBy(c => c.TypeLine).ToList(),
            "power" => desc
                ? allCards.OrderByDescending(c => c.Power).ToList()
                : allCards.OrderBy(c => c.Power).ToList(),
            "toughness" => desc
                ? allCards.OrderByDescending(c => c.Toughness).ToList()
                : allCards.OrderBy(c => c.Toughness).ToList(),
            "artist" => desc
                ? allCards.OrderByDescending(c => c.ArtistId).ToList()
                : allCards.OrderBy(c => c.ArtistId).ToList(),
            "number" => desc
                ? allCards.OrderByDescending(c => c.Number).ToList()
                : allCards.OrderBy(c => c.Number).ToList(),
            _ => allCards.OrderBy(c => c.Number).ToList()
        };

        int totalCount = allCards.Count;
        int offset = (page - 1) * pageSize;
        var paged = allCards.Skip(offset).Take(pageSize).ToList();

        return (paged, totalCount);
    }

    public async Task<Card?> GetByIdAsync(int id)
    {
        return await _orm.GetByIdAsync<Card>("Cards", id);
    }

    public async Task<IReadOnlyList<Card>> SearchAsync(string query, int limit = 10)
    {
        var all = await _orm.GetAllAsync<Card>("Cards");
        string q = query.ToLowerInvariant();

        return all.Where(c => c.Name.ToLowerInvariant().Contains(q))
                  .Take(limit).ToList();
    }

    public async Task<int> InsertAsync(Card card)
    {
        return await _orm.InsertAsync("Cards", card);
    }

    public async Task<int> UpdateAsync(Card card)
    {
        return await _orm.UpdateAsync("Cards", card.Id, card);
    }

    public async Task<int> DeleteAsync(int id)
    {
        return await _orm.DeleteAsync("Cards", id);
    }

    public async Task<IReadOnlyList<Rarity>> GetAllRaritiesAsync()
    {
        return await _orm.GetAllAsync<Rarity>("Rarities");
    }

    public async Task<IReadOnlyList<Artist>> GetAllArtistsAsync()
    {
        return await _orm.GetAllAsync<Artist>("Artists");
    }

    public async Task<int> GetNextNumberAsync(int setId)
    {
        var cards = await _orm.GetAllAsync<Card>("Cards");
        int max = 0;

        foreach (var card in cards)
        {
            if (card.SetId == setId && card.Number > max)
                max = card.Number;
        }

        return max + 1;
    }

    public async Task<IReadOnlyList<Card>> GetAllCardsAsync()
    {
        return await _orm.GetAllAsync<Card>("Cards");
    }
}