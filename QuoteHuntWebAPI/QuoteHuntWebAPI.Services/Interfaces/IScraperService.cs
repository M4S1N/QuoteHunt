using QuoteHuntWebAPI.DTO;
using OpenQA.Selenium;

namespace QuoteHuntWebAPI.Services.Interfaces
{
    public interface IScraperService
    {
        Task<List<QuoteDTO>> ScrapeQuotes(IWebDriver driver, int page, string tag);
        void Login(IWebDriver driver);
    }
}
