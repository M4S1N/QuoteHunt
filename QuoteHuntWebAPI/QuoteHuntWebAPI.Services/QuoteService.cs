using Microsoft.Extensions.Options;
using NUlid;
using QuoteHuntScraper.Services.Interfaces;
using QuoteHuntWebAPI.DTO;
using QuoteHuntWebAPI.Services.Interfaces;
using StackExchange.Redis;

namespace QuoteHuntWebAPI.Services
{
    public class QuoteService(
        ILogger<QuoteService> logger,
        IScraperService scraperService,
        IOptions<RedisSetting> redisSettings,
        IWebDriverFactory webDriverFactory,
        IConnectionMultiplexer? redis = null
    ) : IQuoteService
    {
        public readonly ILogger<QuoteService> _logger = logger;
        public readonly IScraperService _scraperService = scraperService;
        public readonly IDatabase? _redis = redis?.GetDatabase();
        public readonly RedisSetting _redisSettings = redisSettings.Value;
        private readonly IWebDriverFactory _webDriverFactory = webDriverFactory;

        public async Task<IEnumerable<QuoteDTO>> GetQuotesAsync(int page, string tag, CancellationToken cancellationToken = default)
        {
            if (_redis != null)
            {
                var quotes = new List<QuoteDTO>();
                IEnumerable<RedisValue> quoteKeys;

                if (string.IsNullOrEmpty(tag))
                {
                    var server = _redis.Multiplexer.GetServer(_redis.Multiplexer.GetEndPoints().First());
                    quoteKeys = [.. server.Keys(pattern: "quote:*").Select(k => (RedisValue)k.ToString())];
                }
                else
                {
                    quoteKeys = await _redis.SetMembersAsync($"tag:{tag}");
                }

                var skip = (page - 1) * 10;
                var pageKeys = quoteKeys.Skip(skip).Take(10);

                foreach (var key in pageKeys)
                {
                    var entries = await _redis.HashGetAllAsync(key.ToString()!);
                    if (entries.Length == 0) continue;

                    quotes.Add(new QuoteDTO
                    {
                        Id = Ulid.Parse(entries.First(e => e.Name == "Id").Value!),
                        Text = entries.FirstOrDefault(e => e.Name == "Text").Value!,
                        Author = entries.FirstOrDefault(e => e.Name == "Author").Value!,
                        Tags = entries.FirstOrDefault(e => e.Name == "Tags").Value!.ToString()!.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    });
                }

                return quotes;
            }
            else
            {
                using var driver = _webDriverFactory.CreateWebDriver();
                _scraperService.Login(driver);
                return await _scraperService.ScrapeQuotes(driver, page, tag);
            }
        }
    }
}
