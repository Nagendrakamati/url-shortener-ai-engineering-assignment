namespace UrlShortener.Api.Contracts.Responses;

public sealed record UrlAnalyticsResponse(
    string ShortCode,
    string OriginalUrl,
    DateTime CreatedAtUtc,
    long ClickCount,
    DateTime? LastAccessedAtUtc);