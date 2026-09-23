using UrlShortener.Api.Models;
using UrlShortener.Api.Services.Interfaces;

namespace UrlShortener.Tests.TestDoubles;

internal sealed class FakeUrlShortenerService : IUrlShortenerService
{
    public ShortUrl CreatedUrl { get; set; } = new()
    {
        ShortCode = "Code0001",
        OriginalUrl = "https://example.com",
        CreatedAtUtc = new DateTime(2026, 9, 21, 10, 0, 0, DateTimeKind.Utc)
    };

    public string? ResolvedUrl { get; set; }

    public ShortUrl? Analytics { get; set; }

    public Exception? Exception { get; set; }

    public Task<ShortUrl> CreateAsync(string originalUrl, CancellationToken cancellationToken)
    {
        ThrowIfConfigured();
        return Task.FromResult(CreatedUrl);
    }

    public Task<string?> ResolveAsync(string shortCode, CancellationToken cancellationToken)
    {
        ThrowIfConfigured();
        return Task.FromResult(ResolvedUrl);
    }

    public Task<ShortUrl?> GetAnalyticsAsync(string shortCode, CancellationToken cancellationToken)
    {
        ThrowIfConfigured();
        return Task.FromResult(Analytics);
    }

    private void ThrowIfConfigured()
    {
        if (Exception is not null)
        {
            throw Exception;
        }
    }
}