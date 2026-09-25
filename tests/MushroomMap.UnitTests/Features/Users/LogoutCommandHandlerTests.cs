using FluentAssertions;
using Moq;
using MushroomMapApp.Domain.Interfaces;
using MushroomMapApp.Features.Users.Logout;
using Xunit;

namespace MushroomMap.UnitTests.Features.Users;

public class LogoutCommandHandlerTests
{
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly Handler _handler;

    public LogoutCommandHandlerTests()
    {
        _authServiceMock = new Mock<IAuthService>();
        _handler = new Handler(_authServiceMock.Object);
    }

    [Fact]
    public async Task Handle_RevokesRefreshTokens()
    {
        await _handler.Handle(new Command(UserId: 42), CancellationToken.None);

        _authServiceMock.Verify(
            x => x.RevokeRefreshTokens(42, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_DoesNotThrow_WhenAuthServiceSucceeds()
    {
        var act = () => _handler.Handle(new Command(UserId: 7), CancellationToken.None);

        await act.Should().NotThrowAsync();
    }
}
