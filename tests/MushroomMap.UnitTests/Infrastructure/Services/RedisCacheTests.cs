using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using MushroomMapApp.Infrastructure.Services;
using Xunit;

namespace MushroomMap.UnitTests.Infrastructure.Services;

public class RedisCacheTests
{
    private readonly Mock<IDistributedCache> _distributedCacheMock;
    private readonly RedisCache _cache;

    public RedisCacheTests()
    {
        _distributedCacheMock = new Mock<IDistributedCache>();
        _cache = new RedisCache(_distributedCacheMock.Object);
    }

    [Fact]
    public async Task GetAsync_ReturnsDefault_WhenKeyNotFound()
    {
        _distributedCacheMock
            .Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        var result = await _cache.GetAsync<string>("missing-key");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAsync_DeserializesValue()
    {
        var dto = new TestDto { Name = "test", Value = 42 };
        var json = JsonSerializer.Serialize(dto);
        var bytes = Encoding.UTF8.GetBytes(json);

        _distributedCacheMock
            .Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(bytes);

        var result = await _cache.GetAsync<TestDto>("test-key");

        result.Should().NotBeNull();
        result!.Name.Should().Be("test");
        result.Value.Should().Be(42);
    }

    [Fact]
    public async Task SetAsync_SerializesAndStores()
    {
        var dto = new TestDto { Name = "test", Value = 42 };

        _distributedCacheMock
            .Setup(x => x.SetAsync(
                It.IsAny<string>(),
                It.Is<byte[]>(b => Encoding.UTF8.GetString(b).Contains("test")),
                It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _cache.SetAsync("test-key", dto, TimeSpan.FromMinutes(30));

        _distributedCacheMock.Verify(
            x => x.SetAsync(
                "test-key",
                It.Is<byte[]>(b => Encoding.UTF8.GetString(b).Contains("test")),
                It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SetAsync_DoesNotStore_WhenValueIsNull()
    {
        await _cache.SetAsync<string>("test-key", null, TimeSpan.FromMinutes(30));

        _distributedCacheMock.Verify(
            x => x.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task SetAsync_UsesDefaultExpiration_WhenNoTimespan()
    {
        var dto = new TestDto { Name = "test", Value = 42 };

        _distributedCacheMock
            .Setup(x => x.SetAsync(
                It.IsAny<string>(),
                It.IsAny<byte[]>(),
                It.Is<DistributedCacheEntryOptions>(o => o.AbsoluteExpirationRelativeToNow == TimeSpan.FromMinutes(5)),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _cache.SetAsync("test-key", dto, null);
    }

    [Fact]
    public async Task SetAsync_UsesProvidedExpiration()
    {
        var dto = new TestDto { Name = "test", Value = 42 };

        _distributedCacheMock
            .Setup(x => x.SetAsync(
                It.IsAny<string>(),
                It.IsAny<byte[]>(),
                It.Is<DistributedCacheEntryOptions>(o => o.AbsoluteExpirationRelativeToNow == TimeSpan.FromMinutes(30)),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _cache.SetAsync("test-key", dto, TimeSpan.FromMinutes(30));
    }

    private sealed class TestDto
    {
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
    }
}
