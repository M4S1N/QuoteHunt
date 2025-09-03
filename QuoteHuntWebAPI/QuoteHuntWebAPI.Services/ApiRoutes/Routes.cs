namespace QuoteHuntWebAPI.Services.ApiRoutes
{
    public static class Routes
    {
        private const string BasePath = "/internal/Scraper";
        public static string GetQuote(int page, string tag) => $"{BasePath}/quotes?page={page}" + (string.IsNullOrEmpty(tag) ? "" : $"&tag={tag}");
        public static string GetHealth => $"{BasePath}/health";
    }
}
