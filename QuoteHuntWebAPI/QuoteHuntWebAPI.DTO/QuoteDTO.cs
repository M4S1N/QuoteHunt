namespace QuoteHuntWebAPI.DTO
{
    public class QuoteDTO
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public string Author { get; set; }
        public string Source { get; set; }
        public List<string> Tags { get; set; }
    }
}
