using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using UrlShortener.Api.Contracts.Responses;
using UrlShortener.Api.Models;
using UrlShortener.Api.Services.Interfaces;
using UrlShortener.Tests.TestDoubles;

namespace UrlShortener.Tests.Api;

public sealed class ApiContractTests
{
    [Fact]
    public async Task Create_WithInvalidUrl_ReturnsValidationProblemDetails()
    {
        await using var factory = new TestApiFactory(new FakeUrlShortenerService());
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/urls", new { url = "/relative" });
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("absolute HTTP or HTTPS", body);
    }

    [Fact]
    public async Task Redirect_WithKnownCode_ReturnsFoundWithoutFollowingRedirect()
    {
        var service = new FakeUrlShortenerService { ResolvedUrl = "https://example.com" };
        await using var factory = new TestApiFactory(service);
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var response = await client.GetAsync("/Code0001");

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.Equal("https://example.com/", response.Headers.Location?.AbsoluteUri);
    }

    [Fact]
    public async Task Redirect_WithMissingCode_ReturnsNotFound()
    {
        await using var factory = new TestApiFactory(new FakeUrlShortenerService());
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/Missing1");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Analytics_WithKnownCode_ReturnsAggregateResponse()
    {
        var service = new FakeUrlShortenerService
        {
            Analytics = new ShortUrl
            {
                ShortCode = "Code0001",
                OriginalUrl = "https://example.com",
                CreatedAtUtc = new DateTime(2026, 9, 21, 10, 0, 0, DateTimeKind.Utc),
                ClickCount = 3,
                LastAccessedAtUtc = new DateTime(2026, 9, 21, 11, 0, 0, DateTimeKind.Utc)
            }
        };
        await using var factory = new TestApiFactory(service);
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/urls/Code0001/analytics");
        var analytics = await response.Content.ReadFromJsonAsync<UrlAnalyticsResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(analytics);
        Assert.Equal(3, analytics.ClickCount);
    }

    [Fact]
    public async Task Analytics_WithMissingCode_ReturnsNotFound()
    {
        await using var factory = new TestApiFactory(new FakeUrlShortenerService());
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/urls/Missing1/analytics");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UnexpectedException_ReturnsSanitizedProblemDetails()
    {
        var service = new FakeUrlShortenerService
        {
            Exception = new InvalidOperationException("Sensitive internal detail")
        };
        await using var factory = new TestApiFactory(service);
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/urls/Code0001/analytics");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Contains("traceId", body);
        Assert.DoesNotContain("Sensitive internal detail", body);
    }

    [Fact]
    public async Task Liveness_ReturnsOkWithoutCheckingDependencies()
    {
        await using var factory = new TestApiFactory(new FakeUrlShortenerService());
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/health/live");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private sealed class TestApiFactory(FakeUrlShortenerService service)
        : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Production");
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IUrlShortenerService>();
                services.AddSingleton<IUrlShortenerService>(service);
            });
        }
    }
}