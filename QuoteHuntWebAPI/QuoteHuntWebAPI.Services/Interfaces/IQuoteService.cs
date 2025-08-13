using QuoteHuntWebAPI.DTO;

namespace QuoteHuntWebAPI.Services.Interfaces
{
    public interface IQuoteService
    {
        Task<IEnumerable<QuoteDTO>> GetQuotesAsync(int page, string tag, CancellationToken cancellationToken = default);
    }
}
