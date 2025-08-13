using QuoteHuntWebAPI.DTO;
using QuoteHuntWebAPI.Services.Interfaces;
using StackExchange.Redis;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace QuoteHuntWebAPI.Services
{
    public class QuoteService(
        ILogger<QuoteService> logger,
        IScraperClient scraperClient,
        IConnectionMultiplexer redis,
        IOptions<RedisSetting> redisSettings
    ) : IQuoteService
    {
        public readonly ILogger<QuoteService> _logger = logger;
        public readonly IScraperClient _scraperClient = scraperClient;
        public readonly IDatabase _redis = redis.GetDatabase();
        public readonly RedisSetting _redisSettings = redisSettings.Value;

        public async Task<IEnumerable<QuoteDTO>> GetQuotesAsync(int page, string tag, CancellationToken cancellationToken = default)
        {
            string cacheKey = $"quotes:page:{page}:tag:{tag}";

            var cached = await _redis.StringGetAsync(cacheKey);
            if (cached.HasValue)
            {
                return JsonSerializer.Deserialize<IEnumerable<QuoteDTO>>(cached!);
            }

            var quotes = await _scraperClient.GetQuotesAsync(page, tag, cancellationToken);

            var serialized = JsonSerializer.Serialize(quotes);
            await _redis.StringSetAsync(cacheKey, serialized, TimeSpan.FromSeconds(_redisSettings.CacheTTLSeconds));

            return quotes;
        }
    }
}
