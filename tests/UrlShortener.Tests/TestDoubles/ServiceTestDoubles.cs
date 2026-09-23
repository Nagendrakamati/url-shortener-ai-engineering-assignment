using UrlShortener.Api.Models;
using UrlShortener.Api.Repositories.Interfaces;
using UrlShortener.Api.Services.Interfaces;

namespace UrlShortener.Tests.TestDoubles;

internal sealed class SequenceShortCodeGenerator(params string[] codes) : IShortCodeGenerator
{
    private readonly Queue<string> codes = new(codes);

    public string Generate()
    {
        return codes.Dequeue();
    }
}

internal sealed class StubTimeProvider(DateTimeOffset utcNow) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => utcNow;
}

internal sealed class FakeShortUrlRepository : IShortUrlRepository
{
    public Queue<bool> AddResults { get; } = new();

    public List<ShortUrl> AddAttempts { get; } = [];

    public string? OriginalUrl { get; set; }

    public ShortUrl? Analytics { get; set; }

    public List<(string ShortCode, DateTime AccessedAtUtc)> RecordedVisits { get; } = [];

    public Task<bool> TryAddAsync(ShortUrl shortUrl, CancellationToken cancellationToken)
    {
        AddAttempts.Add(shortUrl);
        return Task.FromResult(AddResults.Dequeue());
    }

    public Task<string?> GetOriginalUrlAsync(
        string shortCode,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(OriginalUrl);
    }

    public Task<ShortUrl?> GetAnalyticsAsync(
        string shortCode,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(Analytics);
    }

    public Task RecordVisitAsync(
        string shortCode,
        DateTime accessedAtUtc,
        CancellationToken cancellationToken)
    {
        RecordedVisits.Add((shortCode, accessedAtUtc));
        return Task.CompletedTask;
    }
}