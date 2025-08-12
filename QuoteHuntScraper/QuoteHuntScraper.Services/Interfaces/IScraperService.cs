using OpenQA.Selenium;
using QuoteHuntScraper.DTO;

namespace QuoteHuntScraper.Services.Interfaces
{
    public interface IScraperService
    {
        Task<List<QuoteDTO>> ScrapeQuotes(IWebDriver driver, int page, string tag);
        void Login(IWebDriver driver);
    }
}
