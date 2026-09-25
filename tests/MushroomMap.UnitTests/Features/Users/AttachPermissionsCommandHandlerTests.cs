using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;
using MushroomMap.UnitTests.Common.Database;
using MushroomMapApp.Domain.Data;
using MushroomMapApp.Domain.Entities;
using MushroomMapApp.Domain.Exceptions;
using MushroomMapApp.Domain.Interfaces;
using MushroomMapApp.Domain.Permissions;
using MushroomMapApp.Features.Users.AttachPermissions;
using Xunit;

namespace MushroomMap.UnitTests.Features.Users;

public class AttachPermissionsCommandHandlerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<IPermissionService> _permissionServiceMock;
    private readonly AttachPermissionsCommandHandler _handler;

    public AttachPermissionsCommandHandlerTests()
    {
        _context = TestDbContextFactory.Create();
        _permissionServiceMock = new Mock<IPermissionService>();
        _handler = new AttachPermissionsCommandHandler(_context, _permissionServiceMock.Object);
    }

    public void Dispose() => _context.Dispose();

    private User AddUser()
    {
        var user = new User
        {
            PublicId = Guid.NewGuid(),
            Email = "test@test.com",
            PublicNick = "test",
            FirstName = "test",
            LastName = "test",
            PasswordHash = "hash"
        };

        _context.Users.Add(user);
        return user;
    }

    private Permission AddPermission(string code)
    {
        var permission = new Permission { Code = code, Name = code };
        _context.Permissions.Add(permission);
        return permission;
    }

    private async Task<List<string>> GetUserPermissionCodes(long userId)
    {
        return await _context.UserPermissions
            .Where(x => x.UserId == userId)
            .Select(x => x.Permission.Code)
            .ToListAsync();
    }

    [Fact]
    public async Task Handle_ReplacesUserPermissions_WhenPermissionSetChanges()
    {
        var user = AddUser();
        var view = AddPermission(Permissions.Locations.View.Code);
        var edit = AddPermission(Permissions.Locations.Edit.Code);
        var delete = AddPermission(Permissions.Locations.Delete.Code);
        await _context.SaveChangesAsync();

        _context.UserPermissions.Add(new UserPermission
        {
            UserId = user.Id,
            PermissionId = view.Id
        });
        await _context.SaveChangesAsync();

        var request = new AttachPermissionsRequest(
            UserPublicId: user.PublicId,
            Permissions:
            [
                Permissions.Locations.Edit.Code,
                Permissions.Locations.Delete.Code,
                "unknown.permission"
            ]);

        var result = await _handler.Handle(
            new AttachPermissionsCommand(user.Id, request),
            CancellationToken.None);

        result.Should().Be(Unit.Value);

        var codes = await GetUserPermissionCodes(user.Id);
        codes.Should().BeEquivalentTo(edit.Code, delete.Code);
    }

    [Fact]
    public async Task Handle_KeepsExistingPermissions_WhenRequestedSetIsUnchanged()
    {
        var user = AddUser();
        var view = AddPermission(Permissions.Locations.View.Code);
        await _context.SaveChangesAsync();

        _context.UserPermissions.Add(new UserPermission
        {
            UserId = user.Id,
            PermissionId = view.Id
        });
        await _context.SaveChangesAsync();

        var request = new AttachPermissionsRequest(
            UserPublicId: user.PublicId,
            Permissions: [Permissions.Locations.View.Code]);

        await _handler.Handle(new AttachPermissionsCommand(user.Id, request), CancellationToken.None);

        var codes = await GetUserPermissionCodes(user.Id);
        codes.Should().BeEquivalentTo(view.Code);
    }

    [Fact]
    public async Task Handle_CreatesSinglePermission_WhenSameCodeIsSentTwice()
    {
        var user = AddUser();
        AddPermission(Permissions.Locations.Edit.Code);
        await _context.SaveChangesAsync();

        var request = new AttachPermissionsRequest(
            UserPublicId: user.PublicId,
            Permissions:
            [
                Permissions.Locations.Edit.Code,
                Permissions.Locations.Edit.Code
            ]);

        await _handler.Handle(new AttachPermissionsCommand(user.Id, request), CancellationToken.None);

        var codes = await GetUserPermissionCodes(user.Id);
        codes.Should().ContainSingle().Which.Should().Be(Permissions.Locations.Edit.Code);
    }

    [Fact]
    public async Task Handle_Throws_WhenUserDoesNotExist()
    {
        AddPermission(Permissions.Locations.View.Code);
        await _context.SaveChangesAsync();

        var request = new AttachPermissionsRequest(
            UserPublicId: Guid.NewGuid(),
            Permissions: [Permissions.Locations.View.Code]);

        var act = () => _handler.Handle(
            new AttachPermissionsCommand(1, request),
            CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>();
        (await _context.UserPermissions.ToListAsync()).Should().BeEmpty();
    }
}
