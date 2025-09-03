using NUlid;

namespace QuoteHuntWebAPI.Models
{
    public class Quote
    {
        public Ulid Id { get; set; }
        public required string Text { get; set; }
        public required string Author { get; set; }
    }
}
