using FluentAssertions;
using Moq;
using MushroomMapApp.Domain.Interfaces;
using MushroomMapApp.Infrastructure.Services.Authorization;
using Xunit;

namespace MushroomMap.UnitTests.Infrastructure.Services;

public class PermissionsServiceTests
{
    private readonly Mock<IRedisCache> _redisCacheMock;
    private readonly Mock<IPermissionCacheBuilder> _permissionCacheBuilderMock;
    private readonly PermissionsService _service;

    public PermissionsServiceTests()
    {
        _redisCacheMock = new Mock<IRedisCache>();
        _permissionCacheBuilderMock = new Mock<IPermissionCacheBuilder>();
        _service = new PermissionsService(_permissionCacheBuilderMock.Object, _redisCacheMock.Object);
    }

    [Fact]
    public async Task GetPermissions_ReturnsCached_WhenCacheHit()
    {
        var cached = new HashSet<string> { "locations.view" };

        _redisCacheMock
            .Setup(x => x.GetAsync<HashSet<string>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cached);

        var result = await _service.GetPermissions(1, CancellationToken.None);

        result.Should().BeSameAs(cached);
        _permissionCacheBuilderMock.Verify(
            x => x.Build(It.IsAny<long>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetPermissions_BuildsAndCaches_WhenCacheMiss()
    {
        var permissions = new HashSet<string> { "locations.view", "locations.edit" };

        _redisCacheMock
            .Setup(x => x.GetAsync<HashSet<string>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((HashSet<string>?)null);

        _permissionCacheBuilderMock
            .Setup(x => x.Build(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(permissions);

        var result = await _service.GetPermissions(1, CancellationToken.None);

        result.Should().BeEquivalentTo("locations.view", "locations.edit");

        _redisCacheMock.Verify(
            x => x.SetAsync(
                "permissions:1",
                permissions,
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetPermissions_UsesCorrectCacheKey()
    {
        _redisCacheMock
            .Setup(x => x.GetAsync<HashSet<string>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((HashSet<string>?)null);

        _permissionCacheBuilderMock
            .Setup(x => x.Build(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HashSet<string>());

        await _service.GetPermissions(42, CancellationToken.None);

        _redisCacheMock.Verify(
            x => x.GetAsync<HashSet<string>>("permissions:42", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HasPermission_ReturnsTrue_WhenPermissionExists()
    {
        var permissions = new HashSet<string> { "locations.view" };

        _redisCacheMock
            .Setup(x => x.GetAsync<HashSet<string>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(permissions);

        var result = await _service.HasPermission(1, "locations.view", CancellationToken.None);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasPermission_ReturnsFalse_WhenPermissionMissing()
    {
        var permissions = new HashSet<string> { "locations.view" };

        _redisCacheMock
            .Setup(x => x.GetAsync<HashSet<string>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(permissions);

        var result = await _service.HasPermission(1, "locations.delete", CancellationToken.None);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasPermission_ReturnsFalse_WhenNoPermissions()
    {
        _redisCacheMock
            .Setup(x => x.GetAsync<HashSet<string>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HashSet<string>());

        var result = await _service.HasPermission(1, "locations.view", CancellationToken.None);

        result.Should().BeFalse();
    }
}
