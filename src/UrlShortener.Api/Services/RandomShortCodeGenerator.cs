using System.Security.Cryptography;
using UrlShortener.Api.Services.Interfaces;

namespace UrlShortener.Api.Services;

public sealed class RandomShortCodeGenerator : IShortCodeGenerator
{
    private const string Alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
    private const int CodeLength = 8;

    public string Generate()
    {
        return string.Create(CodeLength, Alphabet, static (characters, alphabet) =>
        {
            for (var index = 0; index < characters.Length; index++)
            {
                characters[index] = alphabet[RandomNumberGenerator.GetInt32(alphabet.Length)];
            }
        });
    }
}