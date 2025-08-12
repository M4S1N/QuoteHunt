using Microsoft.AspNetCore.Mvc;
using QuoteHuntScraper.Services.Interfaces;

namespace QuoteHuntScraper.Server.Controllers
{
    /// <summary>
    /// Controller for quote scraper
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ScraperController(
        ILogger<ScraperController> logger,
        IWebDriverFactory webDriverFactory,
        IScraperService scraperService
    ) : ControllerBase
    {
        private readonly ILogger<ScraperController> _logger = logger;
        private readonly IWebDriverFactory _webDriverFactory = webDriverFactory;
        private readonly IScraperService _scraperService = scraperService;

        [HttpGet("/internal/quotes")]
        public async Task<IActionResult> GetQuote(
            [FromQuery] int? page,
            [FromQuery] IEnumerable<string>? tags,
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                page ??= 1;
                tags ??= [];

                _logger.LogInformation("Scraping quotes from page {Page} with tags: {Tags}", page, string.Join(", ", tags));
                var webDriverFactory = _webDriverFactory.CreateWebDriver();
                using var driver = webDriverFactory;
                var quotes = await _scraperService.ScrapeQuotes(driver, page.Value, [.. tags]);
                if (quotes == null || quotes.Count == 0)
                {
                    _logger.LogWarning("No quotes found on page {Page} with tags: {Tags}", page, string.Join(", ", tags));
                    return NotFound("No quotes found.");
                }
                _logger.LogInformation("Successfully scraped {Count} quotes from page {Page}.", quotes.Count, page);
                return Ok(quotes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while scraping quotes.");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
