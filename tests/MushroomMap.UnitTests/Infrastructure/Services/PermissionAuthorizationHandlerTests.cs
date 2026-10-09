using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Moq;
using MushroomMapApp.Domain.Interfaces;
using MushroomMapApp.Infrastructure.Services.Authorization;
using Xunit;

namespace MushroomMap.UnitTests.Infrastructure.Services;

public class PermissionAuthorizationHandlerTests
{
    private readonly Mock<IPermissionService> _permissionServiceMock;
    private readonly PermissionAuthorizationHandler _handler;

    public PermissionAuthorizationHandlerTests()
    {
        _permissionServiceMock = new Mock<IPermissionService>();
        _handler = new PermissionAuthorizationHandler(_permissionServiceMock.Object);
    }

    private static AuthorizationHandlerContext CreateContext(ClaimsPrincipal user)
        => new([new PermissionRequirement("locations.view")], user, null);

    [Fact]
    public async Task Handle_Succeeds_WhenUserHasPermission()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1")
        }));

        _permissionServiceMock
            .Setup(x => x.HasPermission(1, "locations.view", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var context = CreateContext(user);

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_DoesNotSucceed_WhenUserLacksPermission()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1")
        }));

        _permissionServiceMock
            .Setup(x => x.HasPermission(1, "locations.view", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var context = CreateContext(user);

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_DoesNotSucceed_WhenNoUserIdClaim()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity());

        var context = CreateContext(user);

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
        _permissionServiceMock.Verify(
            x => x.HasPermission(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_DoesNotSucceed_WhenUserIdClaimNotANumber()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "not-a-number")
        }));

        var context = CreateContext(user);

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
        _permissionServiceMock.Verify(
            x => x.HasPermission(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_UsesCorrectPermissionName()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "42")
        }));

        _permissionServiceMock
            .Setup(x => x.HasPermission(42, "locations.delete", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var context = new AuthorizationHandlerContext(
            [new PermissionRequirement("locations.delete")], user, null);

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }
}
