using Microsoft.AspNetCore.Mvc;
using QuoteHuntScraper.Services.Interfaces;

namespace QuoteHuntScraper.Server.Controllers
{
    /// <summary>
    /// Controller for quote scraper
    /// </summary>
    [ApiController]
    [Route("internal/[controller]")]
    public class ScraperController(
        ILogger<ScraperController> logger,
        IWebDriverFactory webDriverFactory,
        IScraperService scraperService
    ) : ControllerBase
    {
        private readonly ILogger<ScraperController> _logger = logger;
        private readonly IWebDriverFactory _webDriverFactory = webDriverFactory;
        private readonly IScraperService _scraperService = scraperService;

        [HttpGet("quotes")]
        public async Task<IActionResult> GetQuote(
            [FromQuery] int? page,
            [FromQuery] string? tag,
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                var webDriverFactory = _webDriverFactory.CreateWebDriver();
                _scraperService.Login(webDriverFactory);

                page ??= 1; tag ??= "";
                _logger.LogInformation("Scraping quotes from page {Page} with tag: {Tag}", page, tag);

                using var driver = webDriverFactory;
                var quotes = await _scraperService.ScrapeQuotes(driver, page.Value, tag);
                if (quotes == null)
                {
                    _logger.LogWarning("Scraping quotes from page {Page} with tag: {Tags}", page, tag);
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
        [HttpGet("health")]
        public IActionResult HealthCheck()
        {
            return Ok("Scraper service is running.");
        }
    }
}
