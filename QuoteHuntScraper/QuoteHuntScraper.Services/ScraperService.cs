using QuoteHuntScraper.Services.Interfaces;
using QuoteHuntScraper.DTO;
using OpenQA.Selenium;
namespace QuoteHuntScraper.Services
{
    public class ScraperService(
        ILogger<ScraperService> logger
    ) : IScraperService
    {
        public readonly ILogger<ScraperService> _logger = logger;

        public Task<List<QuoteDTO>> ScrapeQuotes(IWebDriver driver, int page, List<string> tags)
        {
            driver.Navigate().GoToUrl($"https://quotes.toscrape.com/page/{page}/");

            var quotesElements = driver.FindElements(By.CssSelector(".quote"));
            return Task.FromResult(quotesElements.Select(element => new QuoteDTO
            {
                Text = element.FindElement(By.CssSelector(".text")).Text,
                Author = element.FindElement(By.CssSelector(".author")).Text,
                Tags = [.. element.FindElements(By.CssSelector(".tags .tag")).Select(t => t.Text)]
            }).ToList());
        }
        public void Login(IWebDriver driver, string username, string password)
        {
            driver.Navigate().GoToUrl("https://quotes.toscrape.com/login");
            driver.FindElement(By.Id("username")).SendKeys(username);
            driver.FindElement(By.Id("password")).SendKeys(password);
            driver.FindElement(By.CssSelector("input[type='submit']")).Click();
        }
    }
}
