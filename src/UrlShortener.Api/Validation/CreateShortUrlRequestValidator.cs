using FluentValidation;
using UrlShortener.Api.Contracts.Requests;

namespace UrlShortener.Api.Validation;

public sealed class CreateShortUrlRequestValidator : AbstractValidator<CreateShortUrlRequest>
{
    private const int MaximumUrlLength = 2048;

    public CreateShortUrlRequestValidator()
    {
        RuleFor(request => request.Url)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .MaximumLength(MaximumUrlLength)
            .Must(BeSupportedAbsoluteUrl)
            .WithMessage("URL must be an absolute HTTP or HTTPS URL.");
    }

    private static bool BeSupportedAbsoluteUrl(string? url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var parsedUrl)
            && (parsedUrl.Scheme == Uri.UriSchemeHttp || parsedUrl.Scheme == Uri.UriSchemeHttps);
    }
}