using System.Text.RegularExpressions;
using UrlShortener.Api.Services;

namespace UrlShortener.Tests.Services;

public sealed class RandomShortCodeGeneratorTests
{
    private static readonly Regex Base62Code = new("^[0-9A-Za-z]{8}$");

    [Fact]
    public void Generate_ReturnsEightCharacterBase62Code()
    {
        var generator = new RandomShortCodeGenerator();

        for (var attempt = 0; attempt < 100; attempt++)
        {
            var code = generator.Generate();

            Assert.Matches(Base62Code, code);
        }
    }
}