using Microsoft.EntityFrameworkCore;
using OkToPost.Data;
using OkToPost.Models;

namespace OkToPost.Repositories
{
    public class SqlUrlRepository : IUrlRepository
    {
        private readonly AppDbContext _context;

        public SqlUrlRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<UrlMapping?> GetByCodeAsync(string code) =>
            await _context.UrlMappings.FirstOrDefaultAsync(x => x.ShortCode == code);

        public async Task<UrlMapping?> GetByUrlAsync(string url) =>
            await _context.UrlMappings.FirstOrDefaultAsync(x => x.OriginalUrl == url);

        public async Task AddAsync(UrlMapping mapping)
        {
            await _context.UrlMappings.AddAsync(mapping);
            await _context.SaveChangesAsync();
        }

        public async Task IncrementClickCountAsync(string code)
        {
            var mapping = await GetByCodeAsync(code);
            if (mapping != null)
            {
                mapping.ClickCount++;
                await _context.SaveChangesAsync();
            }
        }
    }
}
