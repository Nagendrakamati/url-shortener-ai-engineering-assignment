namespace UrlShortener.Api.Models;

public sealed class ShortUrl
{
    public long Id { get; set; }

    public required string ShortCode { get; set; }

    public required string OriginalUrl { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public long ClickCount { get; set; }

    public DateTime? LastAccessedAtUtc { get; set; }
}