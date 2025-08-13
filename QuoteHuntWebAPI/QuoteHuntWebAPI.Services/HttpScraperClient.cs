using Microsoft.Extensions.Options;
using QuoteHuntWebAPI.DTO;
using QuoteHuntWebAPI.Services.Interfaces;
using QuoteHuntWebAPI.Services.ApiRoutes;

namespace QuoteHuntWebAPI.Services
{
    public class HttpScraperClient(
        ILogger<HttpScraperClient> logger,
        HttpClient httpClient,
        IOptions<ScraperSetting> scraperSetting
    ): IScraperClient
    {
        private readonly ILogger<HttpScraperClient> _logger = logger;
        private readonly HttpClient _httpClient = new Func<HttpClient>(() =>
        {
            httpClient.BaseAddress = new Uri(scraperSetting.Value.ScraperUrl);
            return httpClient;
        })();

        public async Task<IEnumerable<QuoteDTO>> GetQuotesAsync(int page, string tag, CancellationToken cancellationToken = default)
        {
            try
            {
                using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeoutCts.CancelAfter(TimeSpan.FromSeconds(10));

                string url = Routes.GetQuote(page, tag);
                var response = await _httpClient.GetAsync(url, timeoutCts.Token);
                response.EnsureSuccessStatusCode();

                var quotes = await response.Content.ReadFromJsonAsync<List<QuoteDTO>>(cancellationToken: cancellationToken);
                return quotes ?? Enumerable.Empty<QuoteDTO>();
            }
            catch (OperationCanceledException oce) when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("Request timed out while getting quotes.");
                return [];
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "HTTP error while fetching quotes.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while fetching quotes.");
                throw;
            }
        }

    }
}
