using FluentValidation;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using UrlShortener.Api.Contracts.Requests;
using UrlShortener.Api.Contracts.Responses;
using UrlShortener.Api.Services.Interfaces;

namespace UrlShortener.Api.Controllers;

[ApiController]
[Route("api/urls")]
public sealed class UrlsController(
    IUrlShortenerService urlShortenerService,
    IValidator<CreateShortUrlRequest> createShortUrlValidator,
    ILogger<UrlsController> logger) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<CreateShortUrlResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CreateShortUrlResponse>> Create(
        CreateShortUrlRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await createShortUrlValidator.ValidateAsync(
            request,
            cancellationToken);

        if (!validationResult.IsValid)
        {
            foreach (var error in validationResult.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }

            logger.LogInformation(
                "URL creation rejected with {ValidationErrorCount} validation error(s). TraceId: {TraceId}",
                validationResult.Errors.Count,
                HttpContext.TraceIdentifier);

            return ValidationProblem(ModelState);
        }

        var created = await urlShortenerService.CreateAsync(request.Url!, cancellationToken);
        var redirectUrl = UriHelper.BuildAbsolute(
            Request.Scheme,
            Request.Host,
            Request.PathBase,
            new PathString($"/{created.ShortCode}"));

        var response = new CreateShortUrlResponse(
            created.ShortCode,
            redirectUrl,
            created.OriginalUrl,
            created.CreatedAtUtc);

        logger.LogInformation(
            "Short URL {ShortCode} created. TraceId: {TraceId}",
            created.ShortCode,
            HttpContext.TraceIdentifier);

        return Created(redirectUrl, response);
    }

    [HttpGet("{shortCode:length(8)}/analytics")]
    [ProducesResponseType<UrlAnalyticsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UrlAnalyticsResponse>> GetAnalytics(
        string shortCode,
        CancellationToken cancellationToken)
    {
        var analytics = await urlShortenerService.GetAnalyticsAsync(
            shortCode,
            cancellationToken);

        if (analytics is null)
        {
            logger.LogInformation(
                "Analytics requested for unknown short code {ShortCode}. TraceId: {TraceId}",
                shortCode,
                HttpContext.TraceIdentifier);

            return NotFound();
        }

        logger.LogDebug(
            "Analytics returned for short code {ShortCode}. TraceId: {TraceId}",
            shortCode,
            HttpContext.TraceIdentifier);

        return Ok(new UrlAnalyticsResponse(
            analytics.ShortCode,
            analytics.OriginalUrl,
            analytics.CreatedAtUtc,
            analytics.ClickCount,
            analytics.LastAccessedAtUtc));
    }
}