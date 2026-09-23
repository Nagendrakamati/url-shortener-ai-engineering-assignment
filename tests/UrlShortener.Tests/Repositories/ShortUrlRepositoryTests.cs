using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using UrlShortener.Api.Data;
using UrlShortener.Api.Models;
using UrlShortener.Api.Options;
using UrlShortener.Api.Repositories;
using UrlShortener.Tests.TestDoubles;

namespace UrlShortener.Tests.Repositories;

public sealed class ShortUrlRepositoryTests : IAsyncLifetime
{
    private readonly string databaseName = $"UrlShortener_Tests_{Guid.NewGuid():N}";

    private string ConnectionString =>
        $"Server=(localdb)\\MSSQLLocalDB;Database={databaseName};Trusted_Connection=True;TrustServerCertificate=True";

    public async Task InitializeAsync()
    {
        await using var dbContext = CreateDbContext();
        await dbContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await using var dbContext = CreateDbContext();
        await dbContext.Database.EnsureDeletedAsync();
    }

    [Fact]
    public async Task GetOriginalUrlAsync_ReturnsCachedUrlWithoutSqlRow()
    {
        var cache = CreateMemoryCache();
        await cache.SetStringAsync("short-url:Cached01", "https://cached.example.com");
        await using var dbContext = CreateDbContext();
        var repository = CreateRepository(dbContext, cache);

        var result = await repository.GetOriginalUrlAsync("Cached01", CancellationToken.None);

        Assert.Equal("https://cached.example.com", result);
    }

    [Fact]
    public async Task GetOriginalUrlAsync_OnCacheMissReadsSqlAndPopulatesCache()
    {
        await SeedAsync("SqlMiss1", "https://database.example.com");
        var cache = CreateMemoryCache();
        await using var dbContext = CreateDbContext();
        var repository = CreateRepository(dbContext, cache);

        var result = await repository.GetOriginalUrlAsync("SqlMiss1", CancellationToken.None);

        Assert.Equal("https://database.example.com", result);
        Assert.Equal(
            "https://database.example.com",
            await cache.GetStringAsync("short-url:SqlMiss1"));
    }

    [Fact]
    public async Task GetOriginalUrlAsync_WhenCacheFailsFallsBackToSql()
    {
        await SeedAsync("Failure1", "https://fallback.example.com");
        await using var dbContext = CreateDbContext();
        var repository = CreateRepository(dbContext, new ThrowingDistributedCache());

        var result = await repository.GetOriginalUrlAsync("Failure1", CancellationToken.None);

        Assert.Equal("https://fallback.example.com", result);
    }

    [Fact]
    public async Task TryAddAsync_WhenCacheWriteFailsStillPersistsSqlRow()
    {
        await using var dbContext = CreateDbContext();
        var repository = CreateRepository(dbContext, new ThrowingDistributedCache());
        var shortUrl = CreateShortUrl("WriteErr", "https://persisted.example.com");

        var result = await repository.TryAddAsync(shortUrl, CancellationToken.None);

        Assert.True(result);
        await using var verificationContext = CreateDbContext();
        Assert.True(await verificationContext.ShortUrls.AnyAsync(item => item.ShortCode == "WriteErr"));
    }

    [Fact]
    public async Task RecordVisitAsync_PerformsAtomicAnalyticsUpdates()
    {
        await SeedAsync("Visit001", "https://analytics.example.com");
        await using var dbContext = CreateDbContext();
        var repository = CreateRepository(dbContext, CreateMemoryCache());
        var firstVisit = new DateTime(2026, 9, 21, 10, 0, 0, DateTimeKind.Utc);
        var secondVisit = firstVisit.AddMinutes(1);

        await repository.RecordVisitAsync("Visit001", firstVisit, CancellationToken.None);
        await repository.RecordVisitAsync("Visit001", secondVisit, CancellationToken.None);

        await using var verificationContext = CreateDbContext();
        var analytics = await verificationContext.ShortUrls
            .AsNoTracking()
            .SingleAsync(item => item.ShortCode == "Visit001");
        Assert.Equal(2, analytics.ClickCount);
        Assert.Equal(secondVisit, analytics.LastAccessedAtUtc);
    }

    private UrlShortenerDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<UrlShortenerDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;
        return new UrlShortenerDbContext(options);
    }

    private static IDistributedCache CreateMemoryCache()
    {
        return new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
    }

    private static ShortUrlRepository CreateRepository(
        UrlShortenerDbContext dbContext,
        IDistributedCache cache)
    {
        return new ShortUrlRepository(
            dbContext,
            cache,
            Options.Create(new CacheOptions { ShortUrlExpirationMinutes = 60 }),
            NullLogger<ShortUrlRepository>.Instance);
    }

    private async Task SeedAsync(string shortCode, string originalUrl)
    {
        await using var dbContext = CreateDbContext();
        dbContext.ShortUrls.Add(CreateShortUrl(shortCode, originalUrl));
        await dbContext.SaveChangesAsync();
    }

    private static ShortUrl CreateShortUrl(string shortCode, string originalUrl)
    {
        return new ShortUrl
        {
            ShortCode = shortCode,
            OriginalUrl = originalUrl,
            CreatedAtUtc = DateTime.UtcNow
        };
    }
}