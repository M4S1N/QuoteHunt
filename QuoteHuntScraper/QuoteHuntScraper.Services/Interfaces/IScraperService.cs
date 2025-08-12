using OpenQA.Selenium;
using QuoteHuntScraper.DTO;

namespace QuoteHuntScraper.Services.Interfaces
{
    public interface IScraperService
    {
        Task<List<QuoteDTO>> ScrapeQuotes(IWebDriver driver, int page, List<string> tags);
        void Login(IWebDriver driver, string username, string password);
    }
}
