using FluentAssertions;
using Moq;
using MushroomMapApp.Domain.Interfaces;
using MushroomMapApp.Features.Locations.GetLocations;
using Xunit;

namespace MushroomMap.UnitTests.Features.Locations;

public class LocationsPermissionContextFactoryTests
{
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<IPermissionService> _permissionServiceMock;
    private readonly LocationsPermissionContextFactory _factory;

    public LocationsPermissionContextFactoryTests()
    {
        _currentUserMock = new Mock<ICurrentUser>();
        _permissionServiceMock = new Mock<IPermissionService>();
        _factory = new LocationsPermissionContextFactory(_permissionServiceMock.Object, _currentUserMock.Object);
    }

    [Fact]
    public async Task Create_ReturnsContextWithUserId()
    {
        _currentUserMock.Setup(x => x.UserId).Returns(42);
        _permissionServiceMock
            .Setup(x => x.GetPermissions(42, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HashSet<string> { "locations.view" });

        var context = await _factory.Create(CancellationToken.None);

        context.UserId.Should().Be(42);
    }

    [Fact]
    public async Task Create_ReturnsContextWithPermissions()
    {
        _currentUserMock.Setup(x => x.UserId).Returns(1);
        var permissions = new HashSet<string> { "locations.view", "locations.edit" };
        _permissionServiceMock
            .Setup(x => x.GetPermissions(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(permissions);

        var context = await _factory.Create(CancellationToken.None);

        context.Permissions.Should().BeEquivalentTo("locations.view", "locations.edit");
    }

    [Fact]
    public async Task Create_CallsPermissionService_WithCurrentUserId()
    {
        _currentUserMock.Setup(x => x.UserId).Returns(99);
        _permissionServiceMock
            .Setup(x => x.GetPermissions(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HashSet<string>());

        await _factory.Create(CancellationToken.None);

        _permissionServiceMock.Verify(
            x => x.GetPermissions(99, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Create_ThrowsInvalidOperation_WhenUserIdIsNull()
    {
        _currentUserMock.Setup(x => x.UserId).Returns((long?)null);

        var act = () => _factory.Create(CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
