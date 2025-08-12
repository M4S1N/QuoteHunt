using QuoteHuntWebAPI.DTO;

namespace QuoteHuntWebAPI.Services.Interfaces
{
    public interface IScraperService
    {
        Task<List<QuoteDTO>> ScrapeQuotes(string username, string password, string url, string? quoteId = null);
    }
}
