using Microsoft.Extensions.Logging.Abstractions;
using UrlShortener.Api.Models;
using UrlShortener.Api.Services;
using UrlShortener.Tests.TestDoubles;

namespace UrlShortener.Tests.Services;

public sealed class UrlShortenerServiceTests
{
    private static readonly DateTimeOffset UtcNow = new(2026, 9, 21, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task CreateAsync_RetriesCollisionsAndReturnsSuccessfulCode()
    {
        var repository = new FakeShortUrlRepository();
        repository.AddResults.Enqueue(false);
        repository.AddResults.Enqueue(false);
        repository.AddResults.Enqueue(true);
        var service = new UrlShortenerService(
            repository,
            new SequenceShortCodeGenerator("Collision", "Again123", "Unique12"),
            new StubTimeProvider(UtcNow),
            NullLogger<UrlShortenerService>.Instance);

        var result = await service.CreateAsync("https://example.com", CancellationToken.None);

        Assert.Equal("Unique12", result.ShortCode);
        Assert.Equal(UtcNow.UtcDateTime, result.CreatedAtUtc);
        Assert.Equal(3, repository.AddAttempts.Count);
    }

    [Fact]
    public async Task CreateAsync_ThrowsAfterFiveCollisions()
    {
        var repository = new FakeShortUrlRepository();
        for (var attempt = 0; attempt < 5; attempt++)
        {
            repository.AddResults.Enqueue(false);
        }

        var service = new UrlShortenerService(
            repository,
            new SequenceShortCodeGenerator("Code0001", "Code0002", "Code0003", "Code0004", "Code0005"),
            new StubTimeProvider(UtcNow),
            NullLogger<UrlShortenerService>.Instance);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAsync("https://example.com", CancellationToken.None));

        Assert.Equal("Unable to generate a unique short code.", exception.Message);
        Assert.Equal(5, repository.AddAttempts.Count);
    }

    [Fact]
    public async Task ResolveAsync_RecordsVisitBeforeReturningDestination()
    {
        var repository = new FakeShortUrlRepository
        {
            OriginalUrl = "https://example.com"
        };
        var service = CreateService(repository);

        var result = await service.ResolveAsync("Code0001", CancellationToken.None);

        Assert.Equal("https://example.com", result);
        var visit = Assert.Single(repository.RecordedVisits);
        Assert.Equal("Code0001", visit.ShortCode);
        Assert.Equal(UtcNow.UtcDateTime, visit.AccessedAtUtc);
    }

    [Fact]
    public async Task ResolveAsync_DoesNotRecordVisitWhenCodeIsMissing()
    {
        var repository = new FakeShortUrlRepository();
        var service = CreateService(repository);

        var result = await service.ResolveAsync("Missing1", CancellationToken.None);

        Assert.Null(result);
        Assert.Empty(repository.RecordedVisits);
    }

    [Fact]
    public async Task GetAnalyticsAsync_ReturnsRepositoryResult()
    {
        var expected = new ShortUrl
        {
            ShortCode = "Code0001",
            OriginalUrl = "https://example.com",
            CreatedAtUtc = UtcNow.UtcDateTime,
            ClickCount = 2
        };
        var repository = new FakeShortUrlRepository { Analytics = expected };
        var service = CreateService(repository);

        var result = await service.GetAnalyticsAsync("Code0001", CancellationToken.None);

        Assert.Same(expected, result);
    }

    private static UrlShortenerService CreateService(FakeShortUrlRepository repository)
    {
        return new UrlShortenerService(
            repository,
            new SequenceShortCodeGenerator("Code0001"),
            new StubTimeProvider(UtcNow),
            NullLogger<UrlShortenerService>.Instance);
    }
}