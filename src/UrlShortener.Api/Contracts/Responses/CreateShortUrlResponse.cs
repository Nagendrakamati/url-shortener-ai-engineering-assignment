namespace UrlShortener.Api.Contracts.Responses;

public sealed record CreateShortUrlResponse(
    string ShortCode,
    string ShortUrl,
    string OriginalUrl,
    DateTime CreatedAtUtc);