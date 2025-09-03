using Microsoft.Extensions.Options;
using Moq;
using NUlid;
using QuoteHuntScraper.Services.Interfaces;
using QuoteHuntWebAPI.DTO;
using QuoteHuntWebAPI.Services;
using QuoteHuntWebAPI.Services.Interfaces;
using StackExchange.Redis;
using Xunit;

namespace QuoteHuntWebAPI.UnitTest
{
    public class ScraperBackgroundServiceTests
    {
        [Fact]
        public async Task ExecuteAsync_ScrapesAndSavesQuotes()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ScraperBackgroundService>>();
            var mockScraper = new Mock<IScraperService>();
            var mockWebDriverFactory = new Mock<IWebDriverFactory>();
            var mockDriver = new Mock<OpenQA.Selenium.IWebDriver>();
            mockWebDriverFactory.Setup(f => f.CreateWebDriver()).Returns(mockDriver.Object);

            // Redis
            var mockDb = new Mock<IDatabase>();
            var mockMultiplexer = new Mock<IConnectionMultiplexer>();
            mockMultiplexer.Setup(m => m.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(mockDb.Object);
            var redisSettings = Options.Create(new RedisSetting { CacheTTLSeconds = 1 });

            var services = new ServiceCollection();
            services.AddSingleton(mockScraper.Object);
            services.AddSingleton(mockWebDriverFactory.Object);
            var serviceProvider = services.BuildServiceProvider();

            var quotes = new List<QuoteDTO>
            {
                new() { Id = Ulid.NewUlid(), Text = "Quote 1", Author = "Author 1", Tags = ["tag1"] }
            };
            mockScraper.SetupSequence(s => s.ScrapeQuotes(mockDriver.Object, It.IsAny<int>(), ""))
                       .ReturnsAsync(quotes)
                       .ReturnsAsync([]);

            var backgroundService = new ScraperBackgroundService(
                mockLogger.Object,
                serviceProvider,
                redisSettings,
                mockMultiplexer.Object
            );

            // Act
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));
            await backgroundService.StartAsync(cts.Token);

            // Assert
            mockScraper.Verify(s => s.Login(mockDriver.Object), Times.Once);
            mockScraper.Verify(s => s.ScrapeQuotes(mockDriver.Object, It.IsAny<int>(), ""), Times.AtLeastOnce);
            mockDb.Verify(db => db.SetAddAsync(
                It.IsAny<RedisKey>(),
                It.IsAny<RedisValue>(),
                It.IsAny<CommandFlags>()),
                Times.AtLeastOnce
            );
            mockDb.Verify(db => db.KeyExpireAsync(
                It.IsAny<RedisKey>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<ExpireWhen>(),
                It.IsAny<CommandFlags>()),
                Times.AtLeastOnce
            );
        }
    }
}
