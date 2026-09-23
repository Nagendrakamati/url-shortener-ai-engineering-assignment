using UrlShortener.Api.Models;

namespace UrlShortener.Api.Services.Interfaces;

public interface IUrlShortenerService
{
    Task<ShortUrl> CreateAsync(string originalUrl, CancellationToken cancellationToken);

    Task<string?> ResolveAsync(string shortCode, CancellationToken cancellationToken);

    Task<ShortUrl?> GetAnalyticsAsync(string shortCode, CancellationToken cancellationToken);
}