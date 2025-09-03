using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using QuoteHuntWebAPI.Server.Controllers;
using QuoteHuntWebAPI.DTO;
using QuoteHuntWebAPI.Services.Interfaces;
using Xunit;
using Moq;

namespace QuoteHuntWebAPI.UnitTest
{
    public class QuoteControllerTests
    {
        private static QuoteController CreateController(
            IQuoteService quoteServiceMockImpl
        )
        {
            var loggerMock = new Mock<ILogger<QuoteController>>();

            var optionsMock = new Mock<IOptions<ScraperSetting>>();
            optionsMock.Setup(o => o.Value).Returns(new ScraperSetting
            {
                Url = "https://fake-url.test/"
            });

            return new QuoteController(
                loggerMock.Object,
                optionsMock.Object,
                quoteServiceMockImpl
            );
        }

        [Fact]
        public async Task GetQuotes_ReturnsOk_WhenQuotesExist()
        {
            // Arrange
            var expectedQuotes = new List<QuoteDTO>
            {
                new() { Author = "Test Author", Text = "Test Quote" }
            };

            var quoteServiceMock = new Mock<IQuoteService>();
            quoteServiceMock
                .Setup(s => s.GetQuotesAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedQuotes);

            var controller = CreateController(quoteServiceMock.Object);

            // Act
            var result = await controller.GetQuotes(1, "test");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var quotes = Assert.IsType<IEnumerable<QuoteDTO>>(okResult.Value, exactMatch: false);
            Assert.Single(quotes);
            Assert.Equal("Test Author", quotes.First().Author);
        }

        [Fact]
        public async Task GetQuotes_ReturnsNotFound_WhenNoQuotes()
        {
            // Arrange
            var quoteServiceMock = new Mock<IQuoteService>();
            quoteServiceMock
                .Setup(s => s.GetQuotesAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IEnumerable<QuoteDTO>?)null);

            var controller = CreateController(quoteServiceMock.Object);

            // Act
            var result = await controller.GetQuotes(1, "test");

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("No quotes found.", notFoundResult.Value);
        }

        [Fact]
        public async Task GetQuotes_ReturnsInternalServerError_WhenExceptionIsThrown()
        {
            // Arrange
            var quoteServiceMock = new Mock<IQuoteService>();
            quoteServiceMock
                .Setup(s => s.GetQuotesAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Something went wrong"));

            var controller = CreateController(quoteServiceMock.Object);

            // Act
            var result = await controller.GetQuotes(1, "test");

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
            Assert.Equal("Internal server error", statusCodeResult.Value);
        }
    }
}
