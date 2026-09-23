using UrlShortener.Api.Models;

namespace UrlShortener.Api.Repositories.Interfaces;

public interface IShortUrlRepository
{
    Task<bool> TryAddAsync(ShortUrl shortUrl, CancellationToken cancellationToken);

    Task<string?> GetOriginalUrlAsync(string shortCode, CancellationToken cancellationToken);

    Task<ShortUrl?> GetAnalyticsAsync(string shortCode, CancellationToken cancellationToken);

    Task RecordVisitAsync(
        string shortCode,
        DateTime accessedAtUtc,
        CancellationToken cancellationToken);
}