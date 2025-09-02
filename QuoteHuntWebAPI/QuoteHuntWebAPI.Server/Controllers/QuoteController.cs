using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using QuoteHuntWebAPI.DTO;
using QuoteHuntWebAPI.Services.Interfaces;

namespace QuoteHuntWebAPI.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuoteController(
        ILogger<QuoteController> logger,
        IOptions<ScraperSetting> scraperSetting,
        IQuoteService quoteService
    ) : ControllerBase
    {
        public readonly ILogger<QuoteController> _logger = logger;
        public readonly ScraperSetting _scraperSetting = scraperSetting.Value;
        public readonly IQuoteService _quoteService = quoteService;

        [HttpGet]
        public async Task<IActionResult> GetQuotes(
            [FromQuery] int page = 1,
            [FromQuery] string tag = "",
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                _logger.LogInformation("Fetching quotes from scraper at {ScraperUrl}", _scraperSetting.ScraperUrl);
                
                var quotes = await _quoteService.GetQuotesAsync(page, tag, cancellationToken);
                
                if (quotes == null)
                {
                    return NotFound("No quotes found.");
                }
                return Ok(quotes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching quotes");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }
    }
}
