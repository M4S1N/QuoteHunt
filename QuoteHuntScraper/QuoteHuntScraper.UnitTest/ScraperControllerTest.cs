using Microsoft.AspNetCore.Mvc;
using Moq;
using OpenQA.Selenium;
using QuoteHuntScraper.DTO;
using QuoteHuntScraper.Server.Controllers;
using QuoteHuntScraper.Services.Interfaces;
using Xunit;

namespace QuoteHuntScraper.UnitTest
{
    public class ScraperControllerTest
    {
        private static Mock<IWebDriverFactory> SetupWebDriverFactory()
        {
            var driverMock = new Mock<IWebDriver>();
            var factoryMock = new Mock<IWebDriverFactory>();
            factoryMock
                .Setup(f => f.CreateWebDriver())
                .Returns(driverMock.Object);
            return factoryMock;
        }

        private static ScraperController CreateController(
            Mock<IWebDriverFactory> webDriverFactoryMock,
            Mock<IScraperService> scraperServiceMock
        )
        {
            var loggerMock = new Mock<ILogger<ScraperController>>();
            return new ScraperController(
                loggerMock.Object,
                webDriverFactoryMock.Object,
                scraperServiceMock.Object
            );
        }

        [Fact]
        public async Task GetQuote_ReturnsOk_WhenQuotesFound()
        {
            // Arrange
            var webDriverFactoryMock = SetupWebDriverFactory();
            var scraperServiceMock = new Mock<IScraperService>();

            var expectedQuotes = new List<QuoteDTO>
            {
                new QuoteDTO { Author = "Author 1", Text = "Quote 1" }
            };

            scraperServiceMock
                .Setup(s => s.Login(It.IsAny<IWebDriver>()))
                .Verifiable();

            scraperServiceMock
                .Setup(s => s.ScrapeQuotes(It.IsAny<IWebDriver>(), 1, ""))
                .ReturnsAsync(expectedQuotes);

            var controller = CreateController(webDriverFactoryMock, scraperServiceMock);

            // Act
            var result = await controller.GetQuote(1, "");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var quotes = Assert.IsType<IEnumerable<QuoteDTO>>(okResult.Value, exactMatch: false);
            Assert.Single(quotes);
            Assert.Equal("Author 1", quotes.First().Author);

            scraperServiceMock.Verify(s => s.Login(It.IsAny<IWebDriver>()), Times.Once);
        }

        [Fact]
        public async Task GetQuote_ReturnsNotFound_WhenNoQuotesFound()
        {
            // Arrange
            var webDriverFactoryMock = SetupWebDriverFactory();
            var scraperServiceMock = new Mock<IScraperService>();

            scraperServiceMock
                .Setup(s => s.Login(It.IsAny<IWebDriver>()))
                .Verifiable();

            scraperServiceMock
                .Setup(s => s.ScrapeQuotes(It.IsAny<IWebDriver>(), 1, ""))
                .ReturnsAsync((List<QuoteDTO>?)null);

            var controller = CreateController(webDriverFactoryMock, scraperServiceMock);

            // Act
            var result = await controller.GetQuote(1, "");

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("No quotes found.", notFoundResult.Value);

            scraperServiceMock.Verify(s => s.Login(It.IsAny<IWebDriver>()), Times.Once);
        }

        [Fact]
        public async Task GetQuote_ReturnsInternalServerError_WhenExceptionThrown()
        {
            // Arrange
            var webDriverFactoryMock = SetupWebDriverFactory();
            var scraperServiceMock = new Mock<IScraperService>();

            scraperServiceMock
                .Setup(s => s.Login(It.IsAny<IWebDriver>()))
                .Throws(new InvalidOperationException("Login failed"));

            var controller = CreateController(webDriverFactoryMock, scraperServiceMock);

            // Act
            var result = await controller.GetQuote(1, "");

            // Assert
            var objResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, objResult.StatusCode);
            Assert.Equal("Internal server error", objResult.Value);
        }

        [Fact]
        public void HealthCheck_ReturnsOk()
        {
            // Arrange
            var controller = CreateController(
                SetupWebDriverFactory(),
                new Mock<IScraperService>()
            );

            // Act
            var result = controller.HealthCheck();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Scraper service is running.", okResult.Value);
        }
    }
}
