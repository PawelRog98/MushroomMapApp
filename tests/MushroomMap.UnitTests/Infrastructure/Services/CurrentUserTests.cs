using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using MushroomMapApp.Infrastructure.Services;
using Xunit;

namespace MushroomMap.UnitTests.Infrastructure.Services;

public class CurrentUserTests
{
    [Fact]
    public void UserId_ReturnsId_WhenClaimExists()
    {
        var context = new DefaultHttpContext();
        context.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "42")
        }));

        var currentUser = new CurrentUser(new HttpContextAccessor { HttpContext = context });

        currentUser.UserId.Should().Be(42);
    }

    [Fact]
    public void UserId_ReturnsNull_WhenClaimMissing()
    {
        var context = new DefaultHttpContext();
        context.User = new ClaimsPrincipal(new ClaimsIdentity());

        var currentUser = new CurrentUser(new HttpContextAccessor { HttpContext = context });

        currentUser.UserId.Should().BeNull();
    }

    [Fact]
    public void UserId_ReturnsNull_WhenClaimNotANumber()
    {
        var context = new DefaultHttpContext();
        context.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "not-a-number")
        }));

        var currentUser = new CurrentUser(new HttpContextAccessor { HttpContext = context });

        currentUser.UserId.Should().BeNull();
    }

    [Fact]
    public void UserId_ReturnsNull_WhenHttpContextIsNull()
    {
        var currentUser = new CurrentUser(new HttpContextAccessor { HttpContext = null! });

        currentUser.UserId.Should().BeNull();
    }

    [Fact]
    public void UserId_ReturnsCorrectId_ForLargeNumbers()
    {
        var context = new DefaultHttpContext();
        context.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "9223372036854775807")
        }));

        var currentUser = new CurrentUser(new HttpContextAccessor { HttpContext = context });

        currentUser.UserId.Should().Be(long.MaxValue);
    }
}
