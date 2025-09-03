using Microsoft.Extensions.Options;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using QuoteHuntWebAPI.DTO;
using QuoteHuntWebAPI.Services.Interfaces;
using SeleniumExtras.WaitHelpers;

namespace QuoteHuntScraper.Services
{
    public class ScraperService(
        ILogger<ScraperService> logger,
        IOptions<ScraperSetting> scraperSettings
    ) : IScraperService
    {
        public readonly ILogger<ScraperService> _logger = logger;
        public readonly ScraperSetting _scraperSettings = scraperSettings.Value;

        public Task<List<QuoteDTO>> ScrapeQuotes(IWebDriver driver, int page, string tag)
        {
            string tagPath = string.IsNullOrEmpty(tag) ? "" : $"tag/{tag}/";
            string url = $"{_scraperSettings.Url}/{tagPath}page/{page}/";

            driver.Navigate().GoToUrl(url);

            var quotesElements = driver.FindElements(By.CssSelector(".quote"));
            return Task.FromResult(quotesElements.Select(element => new QuoteDTO
            {
                Text = element.FindElement(By.CssSelector(".text")).Text,
                Author = element.FindElement(By.CssSelector(".author")).Text,
                Tags = [.. element.FindElements(By.CssSelector(".tags .tag")).Select(t => t.Text)]
            }).ToList());
        }
        public void Login(IWebDriver driver)
        {
            var loginUrl = $"{_scraperSettings.Url}/login";

            driver.Navigate().GoToUrl(loginUrl);
            driver.FindElement(By.Id("username")).SendKeys(_scraperSettings.Username);
            driver.FindElement(By.Id("password")).SendKeys(_scraperSettings.Password);

            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
            var loginButton = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("input[type='submit']")));
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", loginButton);
        }
    }
}
