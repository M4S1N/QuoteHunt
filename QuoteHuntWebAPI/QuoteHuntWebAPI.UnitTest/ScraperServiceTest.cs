using Microsoft.Extensions.Options;
using Moq;
using OpenQA.Selenium;
using QuoteHuntScraper.Services;
using QuoteHuntWebAPI.DTO;
using System.Collections.ObjectModel;
using Xunit;

namespace QuoteHuntWebAPI.UnitTest
{
    public class ScraperServiceTests
    {
        private static ScraperService CreateService()
        {
            var loggerMock = new Mock<ILogger<ScraperService>>();
            var optionsMock = new Mock<IOptions<ScraperSetting>>();
            optionsMock.Setup(o => o.Value).Returns(new ScraperSetting
            {
                Url = "https://fake.quotes.toscrape.com",
                Username = "testuser",
                Password = "testpass"
            });

            return new ScraperService(loggerMock.Object, optionsMock.Object);
        }

        [Fact]
        public async Task ScrapeQuotes_ReturnsQuotes()
        {
            // Arrange
            var service = CreateService();

            var driverMock = new Mock<IWebDriver>();
            var navMock = new Mock<INavigation>();
            driverMock.Setup(d => d.Navigate()).Returns(navMock.Object);

            var quoteElementMock = new Mock<IWebElement>();
            var textElementMock = new Mock<IWebElement>();
            var authorElementMock = new Mock<IWebElement>();
            var tagElementMock = new Mock<IWebElement>();

            textElementMock.Setup(e => e.Text).Returns("Quote text");
            authorElementMock.Setup(e => e.Text).Returns("Author name");
            tagElementMock.Setup(e => e.Text).Returns("tag1");

            quoteElementMock
                .Setup(e => e.FindElement(By.CssSelector(".text")))
                .Returns(textElementMock.Object);

            quoteElementMock
                .Setup(e => e.FindElement(By.CssSelector(".author")))
                .Returns(authorElementMock.Object);

            quoteElementMock
                .Setup(e => e.FindElements(By.CssSelector(".tags .tag")))
                .Returns(new ReadOnlyCollection<IWebElement>([tagElementMock.Object]));

            driverMock
                .Setup(d => d.FindElements(By.CssSelector(".quote")))
                .Returns(new ReadOnlyCollection<IWebElement>([quoteElementMock.Object]));

            // Act
            var result = await service.ScrapeQuotes(driverMock.Object, 1, "");

            // Assert
            navMock.Verify(n => n.GoToUrl("https://fake.quotes.toscrape.com/page/1/"), Times.Once);
            Assert.Single(result);
            Assert.Equal("Quote text", result.First().Text);
            Assert.Equal("Author name", result.First().Author);
            Assert.Contains("tag1", result.First().Tags);
        }
    }
}
