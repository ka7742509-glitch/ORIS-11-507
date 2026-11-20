using MiniHttpServer.Models;

namespace MiniHttpServer.Repositories;

public interface ICardRepository
{
    Task<CardSet?> GetSetByIdAsync(int id);
    Task<Rarity?> GetRarityByIdAsync(int id);
    Task<Artist?> GetArtistByIdAsync(int id);
    Task<(IReadOnlyList<Card> Cards, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? sortBy = null, string? sortOrder = null, string? search = null);
    Task<Card?> GetByIdAsync(int id);
    Task<IReadOnlyList<Card>> SearchAsync(string query, int limit = 10);
    Task<int> InsertAsync(Card card);
    Task<int> UpdateAsync(Card card);
    Task<int> DeleteAsync(int id);
    Task<IReadOnlyList<Rarity>> GetAllRaritiesAsync();
    Task<IReadOnlyList<Artist>> GetAllArtistsAsync();
    Task<int> GetNextNumberAsync(int setId);
    Task<IReadOnlyList<Card>> GetAllCardsAsync();
}