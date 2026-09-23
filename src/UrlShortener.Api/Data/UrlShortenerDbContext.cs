using Microsoft.EntityFrameworkCore;
using UrlShortener.Api.Models;

namespace UrlShortener.Api.Data;

public sealed class UrlShortenerDbContext(DbContextOptions<UrlShortenerDbContext> options)
    : DbContext(options)
{
    public DbSet<ShortUrl> ShortUrls => Set<ShortUrl>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var shortUrl = modelBuilder.Entity<ShortUrl>();

        shortUrl.ToTable("ShortUrls");
        shortUrl.HasKey(entity => entity.Id);

        shortUrl.Property(entity => entity.ShortCode)
            .HasMaxLength(10)
            .IsUnicode(false)
            .UseCollation("Latin1_General_100_BIN2")
            .IsRequired();

        shortUrl.HasIndex(entity => entity.ShortCode)
            .IsUnique();

        shortUrl.Property(entity => entity.OriginalUrl)
            .HasMaxLength(2048)
            .IsRequired();

        shortUrl.Property(entity => entity.CreatedAtUtc)
            .HasDefaultValueSql("SYSUTCDATETIME()");

        shortUrl.Property(entity => entity.ClickCount)
            .HasDefaultValue(0L);
    }
}