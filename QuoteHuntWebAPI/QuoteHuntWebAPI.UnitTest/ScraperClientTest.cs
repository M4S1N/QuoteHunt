using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using QuoteHuntWebAPI.DTO;
using QuoteHuntWebAPI.Services;
using System.Net;
using Xunit;

namespace QuoteHuntWebAPI.UnitTest
{
    public class ScraperClientTest
    {
        private static ScraperClient CreateScraperClient(HttpResponseMessage response)
        {
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);

            var httpClient = new HttpClient(handlerMock.Object);

            var loggerMock = new Mock<ILogger<ScraperClient>>();
            var optionsMock = new Mock<IOptions<ScraperSetting>>();
            optionsMock.Setup(o => o.Value).Returns(new ScraperSetting
            {
                ScraperUrl = "https://fake-scraper.test/"
            });

            return new ScraperClient(
                loggerMock.Object,
                httpClient,
                optionsMock.Object
            );
        }

        [Fact]
        public async Task GetQuotesAsync_ReturnsQuotes_WhenResponseIsSuccessful()
        {
            // Arrange
            var expectedQuotes = new List<QuoteDTO>
        {
            new QuoteDTO { Author = "Test Author", Text = "Test Quote" }
        };

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(expectedQuotes)
            };

            var client = CreateScraperClient(response);

            // Act
            var result = await client.GetQuotesAsync(1, "test");

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("Test Author", result.First().Author);
        }

        [Fact]
        public async Task GetQuotesAsync_ReturnsEmpty_WhenRequestTimesOut()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ThrowsAsync(new TaskCanceledException()); // Simula timeout

            var httpClient = new HttpClient(handlerMock.Object);
            var loggerMock = new Mock<ILogger<ScraperClient>>();
            var optionsMock = new Mock<IOptions<ScraperSetting>>();
            optionsMock.Setup(o => o.Value).Returns(new ScraperSetting
            {
                ScraperUrl = "https://fake-scraper.test/"
            });

            var client = new ScraperClient(
                loggerMock.Object,
                httpClient,
                optionsMock.Object
            );

            // Act
            var result = await client.GetQuotesAsync(1, "test");

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetQuotesAsync_ThrowsException_WhenHttpErrorOccurs()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            var client = CreateScraperClient(response);

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => client.GetQuotesAsync(1, "test"));
        }
    }
}
