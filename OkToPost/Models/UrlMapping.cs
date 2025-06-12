using System.ComponentModel.DataAnnotations;

namespace OkToPost.Models
{
    public class UrlMapping
    {
        [Key]
        [MaxLength(20)]
        public required string ShortCode { get; set; } // Primary key for fast lookups and uniqueness
        
        [MaxLength(2048)]
        public required string OriginalUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int ClickCount { get; set; } = 0;
    }
}
