namespace QuoteHuntScraper.DTO
{
    public class QuoteDTO
    {
        public string Text { get; set; }
        public string Author { get; set; }
        public IEnumerable<string> Tags { get; set; }
    }
}
