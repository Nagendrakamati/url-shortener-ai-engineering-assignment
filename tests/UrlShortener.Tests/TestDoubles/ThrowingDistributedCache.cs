using Microsoft.Extensions.Caching.Distributed;

namespace UrlShortener.Tests.TestDoubles;

internal sealed class ThrowingDistributedCache : IDistributedCache
{
    private static InvalidOperationException CreateException() => new("Cache unavailable.");

    public byte[]? Get(string key) => throw CreateException();

    public Task<byte[]?> GetAsync(string key, CancellationToken token = default) =>
        throw CreateException();

    public void Refresh(string key) => throw CreateException();

    public Task RefreshAsync(string key, CancellationToken token = default) =>
        throw CreateException();

    public void Remove(string key) => throw CreateException();

    public Task RemoveAsync(string key, CancellationToken token = default) =>
        throw CreateException();

    public void Set(string key, byte[] value, DistributedCacheEntryOptions options) =>
        throw CreateException();

    public Task SetAsync(
        string key,
        byte[] value,
        DistributedCacheEntryOptions options,
        CancellationToken token = default) => throw CreateException();
}