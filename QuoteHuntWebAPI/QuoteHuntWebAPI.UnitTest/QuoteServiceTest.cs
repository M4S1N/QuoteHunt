using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUlid;
using QuoteHuntScraper.Services.Interfaces;
using QuoteHuntWebAPI.DTO;
using QuoteHuntWebAPI.Services;
using QuoteHuntWebAPI.Services.Interfaces;
using StackExchange.Redis;
using System.Net;
using Xunit;

namespace QuoteHuntWebAPI.UnitTest
{
    public class QuoteServiceTests
    {
        [Fact]
        public async Task GetQuotesAsync_ReturnsQuotes_FromRedis()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<QuoteService>>();
            var mockScraper = new Mock<IScraperService>();
            var mockWebDriverFactory = new Mock<IWebDriverFactory>();

            // Redis mocks
            var mockDb = new Mock<IDatabase>();
            var mockMultiplexer = new Mock<IConnectionMultiplexer>();
            mockMultiplexer.Setup(m => m.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
                           .Returns(mockDb.Object);

            mockDb.Setup(db => db.Multiplexer).Returns(mockMultiplexer.Object);

            var endpoint = new IPEndPoint(IPAddress.Loopback, 6379);
            mockMultiplexer.Setup(m => m.GetEndPoints(It.IsAny<bool>()))
                           .Returns([endpoint]);

            var mockServer = new Mock<IServer>();
            mockMultiplexer.Setup(m => m.GetServer(It.IsAny<EndPoint>(), It.IsAny<object>()))
                           .Returns(mockServer.Object);

            // Keys a devolver
            var quoteId = Ulid.NewUlid();
            mockServer.Setup(s => s.Keys(
                    It.IsAny<int>(),        // database
                    It.IsAny<RedisValue>(), // pattern
                    It.IsAny<int>(),        // pageSize
                    It.IsAny<long>(),       // cursor
                    It.IsAny<int>(),        // pageOffset
                    It.IsAny<CommandFlags>()))
                .Returns([$"quote:{quoteId}"]);

            // Hash de la quote
            mockDb.Setup(db => db.HashGetAllAsync($"quote:{quoteId}", It.IsAny<CommandFlags>()))
                  .ReturnsAsync(
                  [
                      new HashEntry("Id", quoteId.ToString()),
                      new HashEntry("Text", "Test quote"),
                      new HashEntry("Author", "Author 1"),
                      new HashEntry("Tags", "funny,inspirational")
                  ]);

            var options = Options.Create(new RedisSetting { CacheTTLSeconds = 3600 });
            var service = new QuoteService(
                mockLogger.Object,
                mockScraper.Object,
                options,
                mockWebDriverFactory.Object,
                mockMultiplexer.Object
            );

            // Act
            var quotes = (await service.GetQuotesAsync(1, "", CancellationToken.None)).ToList();

            // Assert
            Assert.Single(quotes);
            Assert.Equal("Test quote", quotes[0].Text);
            Assert.Equal("Author 1", quotes[0].Author);
            Assert.Contains("funny", quotes[0].Tags);
            Assert.Contains("inspirational", quotes[0].Tags);
        }

        [Fact]
        public async Task GetQuotesAsync_FallsBackToScraper_WhenRedisIsNull()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<QuoteService>>();
            var mockScraper = new Mock<IScraperService>();
            var mockWebDriverFactory = new Mock<IWebDriverFactory>();
            var mockDriver = new Mock<OpenQA.Selenium.IWebDriver>();
            mockWebDriverFactory.Setup(f => f.CreateWebDriver()).Returns(mockDriver.Object);

            mockScraper.Setup(s => s.ScrapeQuotes(mockDriver.Object, 1, ""))
                       .ReturnsAsync(
                       [
                           new() { Id = Ulid.NewUlid(), Text = "Scraper quote", Author = "Scraper author", Tags = ["tag1"] }
                       ]);

            var options = Options.Create(new RedisSetting { CacheTTLSeconds = 3600 });
            var service = new QuoteService(mockLogger.Object, mockScraper.Object, options, mockWebDriverFactory.Object, null);

            // Act
            var quotes = (await service.GetQuotesAsync(1, "", CancellationToken.None)).ToList();

            // Assert
            Assert.Single(quotes);
            Assert.Equal("Scraper quote", quotes[0].Text);
            Assert.Equal("Scraper author", quotes[0].Author);
            Assert.Contains("tag1", quotes[0].Tags);
            mockScraper.Verify(s => s.ScrapeQuotes(mockDriver.Object, 1, ""), Times.Once);
        }
    }
}
