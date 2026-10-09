using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using MushroomMap.UnitTests.Common.Database;
using MushroomMapApp.Domain.Data;
using MushroomMapApp.Domain.Entities;
using MushroomMapApp.Domain.Interfaces;
using MushroomMapApp.Domain.Permissions;
using MushroomMapApp.Infrastructure.Services.Authorization;
using Xunit;

namespace MushroomMap.UnitTests.Infrastructure.Services;

public class PermissionsSynchronizerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<IPermissionRegistry> _permissionRegistryMock;
    private readonly PermissionsSynchronizer _synchronizer;

    public PermissionsSynchronizerTests()
    {
        _context = TestDbContextFactory.Create();
        _permissionRegistryMock = new Mock<IPermissionRegistry>();

        _permissionRegistryMock
            .Setup(x => x.GetAll())
            .Returns(new[]
            {
                Permissions.Locations.View,
                Permissions.Locations.Edit,
                Permissions.Locations.Delete,
                Permissions.AdministratorDashboard.View,
                Permissions.AdministratorDashboard.PermissionsEdit,
                Permissions.AdministratorDashboard.UserManagment
            });

        _synchronizer = new PermissionsSynchronizer(_context, _permissionRegistryMock.Object);
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task Synchronize_CreatesMissingPermissions()
    {
        await _synchronizer.Synchronize(CancellationToken.None);

        var permissions = await _context.Permissions.ToListAsync();
        permissions.Should().HaveCount(6);
        permissions.Select(p => p.Code).Should().BeEquivalentTo(
            "locations.view", "locations.edit", "locations.delete",
            "administrator.view", "administrator.permissions", "administrator.users");
    }

    [Fact]
    public async Task Synchronize_UpdatesExistingPermissionName()
    {
        _context.Permissions.Add(new Permission
        {
            Code = "locations.view",
            Name = "Old Name",
            IsActive = true
        });
        await _context.SaveChangesAsync();

        await _synchronizer.Synchronize(CancellationToken.None);

        var permission = await _context.Permissions.FirstAsync(p => p.Code == "locations.view");
        permission.Name.Should().Be("View locations");
    }

    [Fact]
    public async Task Synchronize_ReactivatesInactivePermission()
    {
        _context.Permissions.Add(new Permission
        {
            Code = "locations.view",
            Name = "View locations",
            IsActive = false
        });
        await _context.SaveChangesAsync();

        await _synchronizer.Synchronize(CancellationToken.None);

        var permission = await _context.Permissions.FirstAsync(p => p.Code == "locations.view");
        permission.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Synchronize_DeactivatesRemovedPermission()
    {
        _context.Permissions.Add(new Permission
        {
            Code = "old.permission",
            Name = "Old Permission",
            IsActive = true
        });
        await _context.SaveChangesAsync();

        await _synchronizer.Synchronize(CancellationToken.None);

        var permission = await _context.Permissions.FirstAsync(p => p.Code == "old.permission");
        permission.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Synchronize_GrantsAllPermissionsToAdministrator()
    {
        var adminRole = new Role { Name = "Administrator" };
        _context.Roles.Add(adminRole);
        await _context.SaveChangesAsync();

        await _synchronizer.Synchronize(CancellationToken.None);

        var grantedPermissions = await _context.RolePermissions
            .Where(rp => rp.RoleId == adminRole.Id)
            .CountAsync();

        grantedPermissions.Should().Be(6);
    }

    [Fact]
    public async Task Synchronize_DoesNotDuplicateAdminPermissions()
    {
        var adminRole = new Role { Name = "Administrator" };
        _context.Roles.Add(adminRole);
        await _context.SaveChangesAsync();

        await _synchronizer.Synchronize(CancellationToken.None);
        await _synchronizer.Synchronize(CancellationToken.None);

        var grantedPermissions = await _context.RolePermissions
            .Where(rp => rp.RoleId == adminRole.Id)
            .CountAsync();

        grantedPermissions.Should().Be(6);
    }

    [Fact]
    public async Task Synchronize_SkipsAdminGrant_WhenNoAdminRole()
    {
        await _synchronizer.Synchronize(CancellationToken.None);

        var grantedPermissions = await _context.RolePermissions.CountAsync();
        grantedPermissions.Should().Be(0);
    }

    [Fact]
    public async Task Synchronize_DoesNotSave_WhenNoChanges()
    {
        await _synchronizer.Synchronize(CancellationToken.None);

        var permissionsBefore = await _context.Permissions.ToListAsync();

        await _synchronizer.Synchronize(CancellationToken.None);

        var permissionsAfter = await _context.Permissions.ToListAsync();
        permissionsAfter.Should().BeEquivalentTo(permissionsBefore);
    }
}
