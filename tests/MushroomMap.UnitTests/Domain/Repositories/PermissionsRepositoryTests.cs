using FluentAssertions;
using MushroomMap.UnitTests.Common.Database;
using MushroomMapApp.Domain.Data;
using MushroomMapApp.Domain.Entities;
using MushroomMapApp.Domain.Repositories;
using Xunit;

namespace MushroomMap.UnitTests.Domain.Repositories;

public class PermissionsRepositoryTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly PermissionsRepository _repository;

    public PermissionsRepositoryTests()
    {
        _context = TestDbContextFactory.Create();
        _repository = new PermissionsRepository(_context);
    }

    public void Dispose() => _context.Dispose();

    private async Task<User> SeedUserWithPermissionsAsync()
    {
        var role = new Role { Name = "User" };
        _context.Roles.Add(role);

        var permission1 = new Permission { Code = "locations.view", Name = "View locations" };
        var permission2 = new Permission { Code = "locations.edit", Name = "Edit locations" };
        var permission3 = new Permission { Code = "locations.delete", Name = "Delete locations" };
        _context.Permissions.AddRange(permission1, permission2, permission3);
        await _context.SaveChangesAsync();

        var user = new User
        {
            PublicId = Guid.NewGuid(),
            PublicNick = "test",
            FirstName = "Test",
            LastName = "User",
            Email = "test@test.com",
            PasswordHash = "hash",
            RoleId = role.Id
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _context.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id });
        _context.RolePermissions.Add(new RolePermission { RoleId = role.Id, PermissionId = permission1.Id });
        _context.UserPermissions.Add(new UserPermission { UserId = user.Id, PermissionId = permission2.Id });
        await _context.SaveChangesAsync();

        return user;
    }

    [Fact]
    public async Task GetAllPermissionsForUser_ReturnsRoleAndUserPermissions()
    {
        var user = await SeedUserWithPermissionsAsync();

        var result = await _repository.GetAllPermissionsForUser(user.Id, CancellationToken.None);

        result.Should().BeEquivalentTo("locations.view", "locations.edit");
    }

    [Fact]
    public async Task GetAllPermissionsForUser_ReturnsEmpty_WhenUserHasNoPermissions()
    {
        var user = new User
        {
            PublicId = Guid.NewGuid(),
            PublicNick = "test",
            FirstName = "Test",
            LastName = "User",
            Email = "test@test.com",
            PasswordHash = "hash"
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var result = await _repository.GetAllPermissionsForUser(user.Id, CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllPermissionsForUser_DeduplicatesPermissions()
    {
        var role = new Role { Name = "User" };
        _context.Roles.Add(role);

        var permission = new Permission { Code = "locations.view", Name = "View locations" };
        _context.Permissions.Add(permission);
        await _context.SaveChangesAsync();

        var user = new User
        {
            PublicId = Guid.NewGuid(),
            PublicNick = "test",
            FirstName = "Test",
            LastName = "User",
            Email = "test@test.com",
            PasswordHash = "hash",
            RoleId = role.Id
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _context.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id });
        _context.RolePermissions.Add(new RolePermission { RoleId = role.Id, PermissionId = permission.Id });
        _context.UserPermissions.Add(new UserPermission { UserId = user.Id, PermissionId = permission.Id });
        await _context.SaveChangesAsync();

        var result = await _repository.GetAllPermissionsForUser(user.Id, CancellationToken.None);

        result.Should().ContainSingle("locations.view");
    }

    [Fact]
    public async Task GetAllPermissionsForUser_ReturnsOnlyUserPermissions()
    {
        var user = await SeedUserWithPermissionsAsync();

        var result = await _repository.GetAllPermissionsForUser(user.Id, CancellationToken.None);

        result.Should().NotContain("locations.delete");
    }
}
