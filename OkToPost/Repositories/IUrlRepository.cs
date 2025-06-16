using OkToPost.Models;

namespace OkToPost.Repositories
{
    public interface IUrlRepository
    {
        Task<UrlMapping?> GetByCodeAsync(string code);
        Task<UrlMapping?> GetByUrlAsync(string url);
        Task AddAsync(UrlMapping mapping);
        Task IncrementClickCountAsync(string code);
    }

}
