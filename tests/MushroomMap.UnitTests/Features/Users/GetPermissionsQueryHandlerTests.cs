using FluentAssertions;
using Moq;
using MushroomMapApp.Domain.Interfaces;
using MushroomMapApp.Features.Users.GetPermissions;
using Xunit;

namespace MushroomMap.UnitTests.Features.Users;

public class GetPermissionsQueryHandlerTests
{
    private readonly Mock<IPermissionService> _permissionServiceMock;
    private readonly GetPermissionsQueryHandler _handler;

    public GetPermissionsQueryHandlerTests()
    {
        _permissionServiceMock = new Mock<IPermissionService>();
        _handler = new GetPermissionsQueryHandler(_permissionServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ReturnsUserPermissions()
    {
        var permissions = new HashSet<string> { "locations.view", "locations.edit" };

        _permissionServiceMock
            .Setup(x => x.GetPermissions(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(permissions);

        var query = new GetPermissionsQuery(1);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Permissions.Should().BeEquivalentTo("locations.view", "locations.edit");
    }

    [Fact]
    public async Task Handle_ReturnsEmptyList_WhenUserHasNoPermissions()
    {
        _permissionServiceMock
            .Setup(x => x.GetPermissions(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HashSet<string>());

        var query = new GetPermissionsQuery(99);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Permissions.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_CallsPermissionService_WithCorrectUserId()
    {
        _permissionServiceMock
            .Setup(x => x.GetPermissions(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HashSet<string>());

        var query = new GetPermissionsQuery(42);

        await _handler.Handle(query, CancellationToken.None);

        _permissionServiceMock.Verify(
            x => x.GetPermissions(42, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
