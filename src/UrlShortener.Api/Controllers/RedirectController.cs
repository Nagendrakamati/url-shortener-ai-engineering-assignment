using Microsoft.AspNetCore.Mvc;
using UrlShortener.Api.Services.Interfaces;

namespace UrlShortener.Api.Controllers;

[ApiController]
[Route("")]
public sealed class RedirectController(
    IUrlShortenerService urlShortenerService,
    ILogger<RedirectController> logger) : ControllerBase
{
    [HttpGet("{shortCode:length(8)}")]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RedirectToOriginalUrl(
        string shortCode,
        CancellationToken cancellationToken)
    {
        var originalUrl = await urlShortenerService.ResolveAsync(shortCode, cancellationToken);

        if (originalUrl is null)
        {
            logger.LogInformation(
                "Redirect requested for unknown short code {ShortCode}. TraceId: {TraceId}",
                shortCode,
                HttpContext.TraceIdentifier);

            return NotFound();
        }

        logger.LogDebug(
            "Redirect resolved for short code {ShortCode}. TraceId: {TraceId}",
            shortCode,
            HttpContext.TraceIdentifier);

        return Redirect(originalUrl);
    }
}