using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using QuoteHuntScraper.Services.Interfaces;

namespace QuoteHuntScraper.Services
{
    public class WebDriverFactory : IWebDriverFactory
    {
        /// <summary>
        /// Creates a new instance of the ChromeDriver with specified options.
        /// </summary>
        /// <returns>A new instance of ChromeDriver.</returns>
        public IWebDriver CreateWebDriver()
        {
            var options = new ChromeOptions();
            options.AddArgument("--headless");  // Run without UI
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
            return new ChromeDriver(options);
        }
    }
}
