using FluentAssertions;
using Moq;
using MushroomMapApp.Domain.Interfaces;
using MushroomMapApp.Domain.Models;
using MushroomMapApp.Features.Users.Refresh;
using Xunit;

namespace MushroomMap.UnitTests.Features.Users;

public class RefreshCommandHandlerTests
{
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly Handler _handler;

    public RefreshCommandHandlerTests()
    {
        _authServiceMock = new Mock<IAuthService>();
        _handler = new Handler(_authServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ReturnsMappedToken_WhenRefreshTokenIsValid()
    {
        _authServiceMock
            .Setup(x => x.RefreshToken("refresh-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AuthTokenModel(
                AccessToken: "new-access-token",
                RefreshToken: "new-refresh-token",
                UserNick: "test"));

        var command = new Command(new RefreshRequest(RefreshToken: "refresh-token"));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.AccessToken.Should().Be("new-access-token");
        result.RefreshToken.Should().Be("new-refresh-token");
        result.UserNick.Should().Be("test");

        _authServiceMock.Verify(
            x => x.RefreshToken("refresh-token", It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
