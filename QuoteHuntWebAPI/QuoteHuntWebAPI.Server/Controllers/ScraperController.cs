using Microsoft.AspNetCore.Mvc;

namespace QuoteHuntWebAPI.Server.Controllers
{
    /// <summary>
    /// Controller for quote scraper
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ScraperController(
        ILogger<ScraperController> logger
    ) : ControllerBase
    {
        private readonly ILogger<ScraperController> _logger = logger;

        [HttpGet("")]
        public async Task<IActionResult> GetQuote(CancellationToken cancellationToken = default)
        {
            try
            {
                return Ok("This is a placeholder for the quote scraping functionality.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while scraping quotes.");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
