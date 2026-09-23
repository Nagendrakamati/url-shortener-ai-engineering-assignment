using UrlShortener.Api.Contracts.Requests;
using UrlShortener.Api.Validation;

namespace UrlShortener.Tests.Validation;

public sealed class CreateShortUrlRequestValidatorTests
{
    private readonly CreateShortUrlRequestValidator validator = new();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("/relative")]
    [InlineData("ftp://example.com/file")]
    [InlineData("example.com")]
    public void Validate_RejectsInvalidUrl(string? url)
    {
        var result = validator.Validate(new CreateShortUrlRequest(url));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "Url");
    }

    [Theory]
    [InlineData("http://example.com")]
    [InlineData("https://example.com/path?value=1")]
    public void Validate_AcceptsSupportedAbsoluteUrl(string url)
    {
        var result = validator.Validate(new CreateShortUrlRequest(url));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_RejectsUrlLongerThanDatabaseColumn()
    {
        var url = $"https://example.com/{new string('a', 2048)}";

        var result = validator.Validate(new CreateShortUrlRequest(url));

        Assert.Contains(result.Errors, error => error.ErrorCode == "MaximumLengthValidator");
    }
}