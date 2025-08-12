using QuoteHuntWebAPI.Services.Interfaces;
using QuoteHuntWebAPI.DTO;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace QuoteHuntWebAPI.Services
{
    public class ScraperService(
        ILogger<ScraperService> logger
    ) : IScraperService
    {
        public readonly ILogger<ScraperService> _logger = logger;
        public Task<List<QuoteDTO>> ScrapeQuotes(string username, string password, string url, string? quoteId = null)
        {
            var options = new ChromeOptions();
            options.AddArgument("--headless");  // Run without UI
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");

            using var driver = new ChromeDriver(options);

            // Navigate to login
            driver.Navigate().GoToUrl(url);

            // Complete login
            driver.FindElement(By.Id("username")).SendKeys(username);
            driver.FindElement(By.Id("password")).SendKeys(password);
            driver.FindElement(By.CssSelector("input[type='submit']")).Click();

            // Wait for the main page to load (you can improve this with WebDriverWait)
            Thread.Sleep(2000);

            // Navigate to page 1 of quotes
            driver.Navigate().GoToUrl("https://quotes.toscrape.com/page/1/");

            var quotesElements = driver.FindElements(By.CssSelector(".quote"));
            var quotes = new List<QuoteDTO>();

            foreach (var element in quotesElements)
            {
                var text = element.FindElement(By.CssSelector(".text")).Text;
                var author = element.FindElement(By.CssSelector(".author")).Text;
                var tagsElements = element.FindElements(By.CssSelector(".tags .tag"));
                var tags = tagsElements.Select(t => t.Text).ToList();

                quotes.Add(new QuoteDTO { Text = text, Author = author, Tags = tags });
            }

            return Task.FromResult(quotes);
        }
    }
}
