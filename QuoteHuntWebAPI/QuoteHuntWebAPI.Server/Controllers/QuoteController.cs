using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using QuoteHuntWebAPI.DTO;

namespace QuoteHuntWebAPI.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuoteController(
        ILogger<QuoteController> logger,
        IOptions<ScraperSetting> scraperSetting
    ) : ControllerBase
    {
        public readonly ILogger<QuoteController> _logger = logger;
        public readonly ScraperSetting _scraperSetting = scraperSetting.Value;
    }
}
