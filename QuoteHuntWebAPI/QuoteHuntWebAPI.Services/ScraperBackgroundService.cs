using Microsoft.Extensions.Options;
using QuoteHuntScraper.Services.Interfaces;
using QuoteHuntWebAPI.DTO;
using QuoteHuntWebAPI.Services.Interfaces;
using StackExchange.Redis;

namespace QuoteHuntWebAPI.Services
{
    public class ScraperBackgroundService(
        ILogger<ScraperBackgroundService> logger,
        IServiceProvider serviceProvider,
        IOptions<RedisSetting> redisSettings,
        IConnectionMultiplexer? redis = null
    ) : BackgroundService
    {
        private readonly ILogger<ScraperBackgroundService> _logger = logger;
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        public readonly IDatabase? _redis = redis?.GetDatabase();
        public readonly RedisSetting _redisSettings = redisSettings.Value;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var scraperService = scope.ServiceProvider.GetRequiredService<IScraperService>();
            var webDriverFactory = scope.ServiceProvider.GetRequiredService<IWebDriverFactory>();

            _logger.LogInformation("Background service is starting.");
            stoppingToken.Register(() => _logger.LogInformation("Background service is stopping."));

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var driver = webDriverFactory.CreateWebDriver();
                    scraperService.Login(driver);

                    for (int page = 1; ; page++)
                    {
                        var quotes = await scraperService.ScrapeQuotes(driver, page, "");
                        
                        if (quotes == null || quotes.Count == 0)
                        {
                            _logger.LogInformation("No more quotes found. Ending scraping.");
                            break;
                        }

                        _logger.LogInformation("Scraped {Count} quotes from page {Page}.", quotes.Count, page);

                        SaveQuotes(quotes, stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while scraping quotes.");
                }
                await Task.Delay(TimeSpan.FromSeconds(_redisSettings.CacheTTLSeconds), stoppingToken);
            }
            _logger.LogInformation("Background service has stopped.");
        }

        private async void SaveQuotes(IEnumerable<QuoteDTO> quotes, CancellationToken cancellationToken = default)
        {
            if (_redis != null)
            {
                foreach (var quote in quotes)
                {
                    var quoteKey = $"quote:{quote.Id}";
                    await _redis.HashSetAsync(quoteKey,
                    [
                        new("Id", quote.Id.ToString()),
                        new("Text", quote.Text),
                        new("Author", quote.Author),
                        new("Tags", string.Join(",", quote.Tags ?? []))
                    ]);
                    await _redis.KeyExpireAsync(quoteKey, TimeSpan.FromSeconds(_redisSettings.CacheTTLSeconds));

                    foreach (var tag in quote.Tags ?? [])
                    {
                        var key = $"tag:{tag}";
                        await _redis.SetAddAsync(key, quoteKey);
                        await _redis.KeyExpireAsync(key, TimeSpan.FromSeconds(_redisSettings.CacheTTLSeconds));
                    }
                }
            }
            else
            {
                _logger.LogWarning("Redis database is not available. Skipping quote storage.");
            }
        }
    }
}
