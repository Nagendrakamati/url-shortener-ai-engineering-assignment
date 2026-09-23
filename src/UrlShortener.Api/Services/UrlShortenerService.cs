using UrlShortener.Api.Models;
using UrlShortener.Api.Repositories.Interfaces;
using UrlShortener.Api.Services.Interfaces;

namespace UrlShortener.Api.Services;

public sealed class UrlShortenerService(
    IShortUrlRepository shortUrlRepository,
    IShortCodeGenerator shortCodeGenerator,
    TimeProvider timeProvider,
    ILogger<UrlShortenerService> logger) : IUrlShortenerService
{
    private const int MaximumGenerationAttempts = 5;

    public async Task<ShortUrl> CreateAsync(
        string originalUrl,
        CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < MaximumGenerationAttempts; attempt++)
        {
            var shortUrl = new ShortUrl
            {
                ShortCode = shortCodeGenerator.Generate(),
                OriginalUrl = originalUrl,
                CreatedAtUtc = timeProvider.GetUtcNow().UtcDateTime
            };

            if (await shortUrlRepository.TryAddAsync(shortUrl, cancellationToken))
            {
                logger.LogDebug(
                    "Short code {ShortCode} persisted on generation attempt {GenerationAttempt}.",
                    shortUrl.ShortCode,
                    attempt + 1);

                return shortUrl;
            }

            logger.LogWarning(
                "Short code collision for {ShortCode} on generation attempt {GenerationAttempt} of {MaximumGenerationAttempts}.",
                shortUrl.ShortCode,
                attempt + 1,
                MaximumGenerationAttempts);
        }

        logger.LogError(
            "Unable to generate a unique short code after {MaximumGenerationAttempts} attempts.",
            MaximumGenerationAttempts);

        throw new InvalidOperationException("Unable to generate a unique short code.");
    }

    public async Task<string?> ResolveAsync(
        string shortCode,
        CancellationToken cancellationToken)
    {
        var originalUrl = await shortUrlRepository.GetOriginalUrlAsync(
            shortCode,
            cancellationToken);

        if (originalUrl is null)
        {
            logger.LogDebug("Short code {ShortCode} could not be resolved.", shortCode);
            return null;
        }

        var accessedAtUtc = timeProvider.GetUtcNow().UtcDateTime;

        await shortUrlRepository.RecordVisitAsync(
            shortCode,
            accessedAtUtc,
            cancellationToken);

        logger.LogDebug(
            "Visit recorded for short code {ShortCode} at {AccessedAtUtc}.",
            shortCode,
            accessedAtUtc);

        return originalUrl;
    }

    public Task<ShortUrl?> GetAnalyticsAsync(
        string shortCode,
        CancellationToken cancellationToken)
    {
        return GetAnalyticsInternalAsync(shortCode, cancellationToken);
    }

    private async Task<ShortUrl?> GetAnalyticsInternalAsync(
        string shortCode,
        CancellationToken cancellationToken)
    {
        var analytics = await shortUrlRepository.GetAnalyticsAsync(
            shortCode,
            cancellationToken);

        if (analytics is null)
        {
            logger.LogDebug("Analytics not found for short code {ShortCode}.", shortCode);
        }

        return analytics;
    }
}