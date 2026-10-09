using FluentAssertions;
using MushroomMapApp.Domain.Permissions;
using MushroomMapApp.Infrastructure.Services.Authorization;
using Xunit;

namespace MushroomMap.UnitTests.Infrastructure.Services;

public class PermissionRegistryTests
{
    private readonly PermissionRegistry _registry = new();

    [Fact]
    public void GetAll_ReturnsAllDefinedPermissions()
    {
        var all = _registry.GetAll().ToList();

        all.Should().HaveCount(6);
        all.Select(p => p.Code).Should().BeEquivalentTo(
            "locations.view", "locations.edit", "locations.delete",
            "administrator.view", "administrator.permissions", "administrator.users");
    }

    [Fact]
    public void GetExpanded_ReturnsSelf_ForLeafPermission()
    {
        var expanded = _registry.GetExpanded("locations.view");

        expanded.Should().ContainSingle("locations.view");
    }

    [Fact]
    public void GetExpanded_ReturnsImpliedPermissions()
    {
        var expanded = _registry.GetExpanded("locations.delete");

        expanded.Should().BeEquivalentTo("locations.delete", "locations.edit", "locations.view");
    }

    [Fact]
    public void GetExpanded_AdministratorPermissions_ImplyView()
    {
        var expanded = _registry.GetExpanded("administrator.permissions");

        expanded.Should().BeEquivalentTo("administrator.permissions", "administrator.view");
    }

    [Fact]
    public void GetExpanded_AdministratorUsers_ImpliesView()
    {
        var expanded = _registry.GetExpanded("administrator.users");

        expanded.Should().BeEquivalentTo("administrator.users", "administrator.view");
    }

    [Fact]
    public void GetExpanded_Throws_ForUnknownPermission()
    {
        var act = () => _registry.GetExpanded("unknown.permission");

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*unknown.permission*");
    }

    [Fact]
    public void Constructor_Throws_WhenPermissionImpliesUnknown()
    {
        var act = () => new PermissionRegistry();

        act.Should().NotThrow();
    }
}
