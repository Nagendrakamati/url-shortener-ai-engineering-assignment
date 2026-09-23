using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using UrlShortener.Api.Data;
using UrlShortener.Api.Models;
using UrlShortener.Api.Options;
using UrlShortener.Api.Repositories.Interfaces;

namespace UrlShortener.Api.Repositories;

public sealed class ShortUrlRepository(
    UrlShortenerDbContext dbContext,
    IDistributedCache cache,
    IOptions<CacheOptions> cacheOptions,
    ILogger<ShortUrlRepository> logger) : IShortUrlRepository
{
    private const string CacheKeyPrefix = "short-url:";

    private readonly DistributedCacheEntryOptions cacheEntryOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(
            cacheOptions.Value.ShortUrlExpirationMinutes)
    };

    public async Task<bool> TryAddAsync(
        ShortUrl shortUrl,
        CancellationToken cancellationToken)
    {
        dbContext.ShortUrls.Add(shortUrl);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            await TrySetCachedUrlAsync(
                shortUrl.ShortCode,
                shortUrl.OriginalUrl,
                cancellationToken);
            return true;
        }
        catch (DbUpdateException exception) when (IsUniqueConstraintViolation(exception))
        {
            dbContext.Entry(shortUrl).State = EntityState.Detached;
            return false;
        }
    }

    public async Task<string?> GetOriginalUrlAsync(
        string shortCode,
        CancellationToken cancellationToken)
    {
        var cachedUrl = await TryGetCachedUrlAsync(shortCode, cancellationToken);

        if (cachedUrl is not null)
        {
            return cachedUrl;
        }

        var originalUrl = await dbContext.ShortUrls
            .AsNoTracking()
            .Where(shortUrl => shortUrl.ShortCode == shortCode)
            .Select(shortUrl => shortUrl.OriginalUrl)
            .SingleOrDefaultAsync(cancellationToken);

        if (originalUrl is not null)
        {
            await TrySetCachedUrlAsync(shortCode, originalUrl, cancellationToken);
        }

        return originalUrl;
    }

    public Task<ShortUrl?> GetAnalyticsAsync(
        string shortCode,
        CancellationToken cancellationToken)
    {
        return dbContext.ShortUrls
            .AsNoTracking()
            .SingleOrDefaultAsync(
                shortUrl => shortUrl.ShortCode == shortCode,
                cancellationToken);
    }

    public async Task RecordVisitAsync(
        string shortCode,
        DateTime accessedAtUtc,
        CancellationToken cancellationToken)
    {
        await dbContext.ShortUrls
            .Where(shortUrl => shortUrl.ShortCode == shortCode)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(shortUrl => shortUrl.ClickCount, shortUrl => shortUrl.ClickCount + 1)
                    .SetProperty(shortUrl => shortUrl.LastAccessedAtUtc, accessedAtUtc),
                cancellationToken);
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException exception)
    {
        return exception.GetBaseException() is SqlException { Number: 2601 or 2627 };
    }

    private async Task<string?> TryGetCachedUrlAsync(
        string shortCode,
        CancellationToken cancellationToken)
    {
        try
        {
            return await cache.GetStringAsync(
                BuildCacheKey(shortCode),
                cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                exception,
                "Redis read failed for short code {ShortCode}; falling back to SQL Server.",
                shortCode);
            return null;
        }
    }

    private async Task TrySetCachedUrlAsync(
        string shortCode,
        string originalUrl,
        CancellationToken cancellationToken)
    {
        try
        {
            await cache.SetStringAsync(
                BuildCacheKey(shortCode),
                originalUrl,
                cacheEntryOptions,
                cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                exception,
                "Redis write failed for short code {ShortCode}; continuing with SQL Server.",
                shortCode);
        }
    }

    private static string BuildCacheKey(string shortCode)
    {
        return $"{CacheKeyPrefix}{shortCode}";
    }
}