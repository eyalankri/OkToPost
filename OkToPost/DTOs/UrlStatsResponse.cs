namespace OkToPost.DTOs
{
    public class UrlStatsResponse
    {
        public required string Code { get; set; }
        public int Clicks { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
