using OkToPost.Models;
using Microsoft.EntityFrameworkCore;

namespace OkToPost.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<UrlMapping> UrlMappings { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Enforce unique non-clustered index on OriginalUrl
            modelBuilder.Entity<UrlMapping>()
                .HasIndex(u => u.OriginalUrl)
                .IsUnique();

            // Optionally enforce max length from the model here (redundant if already in attributes)
            modelBuilder.Entity<UrlMapping>()
                .Property(u => u.OriginalUrl)
                .HasMaxLength(2048);

            base.OnModelCreating(modelBuilder);
        }
    }
}
