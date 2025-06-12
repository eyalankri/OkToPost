using OkToPost.DTOs;
using static UrlShortenerService;

namespace OkToPost.Services
{
    public interface IUrlShortenerService
    {
        Task<UrlResolutionResult> ShortenUrlAsync(string url);
        Task<UrlResolutionResult> GetOriginalUrlAsync(string code);
        Task<UrlStatsResult> GetStatsAsync(string code);
    }
}
