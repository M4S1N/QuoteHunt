using OpenQA.Selenium;

namespace QuoteHuntScraper.Services.Interfaces
{
    public interface IWebDriverFactory
    {
        /// <summary>
        /// Creates a new instance of the ChromeDriver with specified options.
        /// </summary>
        /// <returns>A new instance of ChromeDriver.</returns>
        IWebDriver CreateWebDriver();
    }
}
