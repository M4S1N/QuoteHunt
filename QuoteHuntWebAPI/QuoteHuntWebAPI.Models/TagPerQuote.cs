using NUlid;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuoteHuntWebAPI.Models
{
    public class TagPerQuote
    {
        [ForeignKey(nameof(Quote))]
        public required Ulid QuoteId { get; set; }
        public Quote Quote { get; set; } = null!;
        public required string Tag { get; set; }
    }
}
