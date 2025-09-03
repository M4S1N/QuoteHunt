using NUlid;

namespace QuoteHuntWebAPI.DTO
{
    public class QuoteDTO
    {
        public Ulid Id { get; set; } = Ulid.NewUlid();
        public string Text { get; set; }
        public string Author { get; set; }
        public IEnumerable<string> Tags { get; set; }
    }
}
