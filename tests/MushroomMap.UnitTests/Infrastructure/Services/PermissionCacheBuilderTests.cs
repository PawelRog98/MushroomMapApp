using FluentAssertions;
using Moq;
using MushroomMapApp.Domain.Interfaces;
using MushroomMapApp.Domain.Permissions;
using MushroomMapApp.Infrastructure.Services.Authorization;
using Xunit;

namespace MushroomMap.UnitTests.Infrastructure.Services;

public class PermissionCacheBuilderTests
{
    private readonly Mock<IPermissionsRepository> _permissionsRepositoryMock;
    private readonly Mock<IPermissionRegistry> _permissionRegistryMock;
    private readonly PermissionCacheBuilder _builder;

    public PermissionCacheBuilderTests()
    {
        _permissionsRepositoryMock = new Mock<IPermissionsRepository>();
        _permissionRegistryMock = new Mock<IPermissionRegistry>();
        _builder = new PermissionCacheBuilder(_permissionsRepositoryMock.Object, _permissionRegistryMock.Object);
    }

    [Fact]
    public async Task Build_ReturnsExpandedPermissions()
    {
        var assigned = new List<string> { "locations.delete" };

        _permissionsRepositoryMock
            .Setup(x => x.GetAllPermissionsForUser(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(assigned);

        _permissionRegistryMock
            .Setup(x => x.GetExpanded("locations.delete"))
            .Returns(new HashSet<string> { "locations.delete", "locations.edit", "locations.view" });

        var result = await _builder.Build(1, CancellationToken.None);

        result.Should().BeEquivalentTo("locations.delete", "locations.edit", "locations.view");
    }

    [Fact]
    public async Task Build_ExpandsMultiplePermissions()
    {
        var assigned = new List<string> { "locations.delete", "administrator.view" };

        _permissionsRepositoryMock
            .Setup(x => x.GetAllPermissionsForUser(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(assigned);

        _permissionRegistryMock
            .Setup(x => x.GetExpanded("locations.delete"))
            .Returns(new HashSet<string> { "locations.delete", "locations.edit", "locations.view" });

        _permissionRegistryMock
            .Setup(x => x.GetExpanded("administrator.view"))
            .Returns(new HashSet<string> { "administrator.view" });

        var result = await _builder.Build(1, CancellationToken.None);

        result.Should().BeEquivalentTo(
            "locations.delete", "locations.edit", "locations.view", "administrator.view");
    }

    [Fact]
    public async Task Build_ReturnsEmpty_WhenNoPermissionsAssigned()
    {
        _permissionsRepositoryMock
            .Setup(x => x.GetAllPermissionsForUser(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string>());

        var result = await _builder.Build(1, CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Build_DeduplicatesPermissions()
    {
        var assigned = new List<string> { "locations.delete", "locations.edit" };

        _permissionsRepositoryMock
            .Setup(x => x.GetAllPermissionsForUser(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(assigned);

        _permissionRegistryMock
            .Setup(x => x.GetExpanded("locations.delete"))
            .Returns(new HashSet<string> { "locations.delete", "locations.edit", "locations.view" });

        _permissionRegistryMock
            .Setup(x => x.GetExpanded("locations.edit"))
            .Returns(new HashSet<string> { "locations.edit", "locations.view" });

        var result = await _builder.Build(1, CancellationToken.None);

        result.Should().BeEquivalentTo("locations.delete", "locations.edit", "locations.view");
        result.Should().OnlyHaveUniqueItems();
    }
}
