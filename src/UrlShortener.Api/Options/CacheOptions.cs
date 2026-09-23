namespace UrlShortener.Api.Options;

public sealed class CacheOptions
{
    public const string SectionName = "Cache";

    public int ShortUrlExpirationMinutes { get; init; } = 60;
}