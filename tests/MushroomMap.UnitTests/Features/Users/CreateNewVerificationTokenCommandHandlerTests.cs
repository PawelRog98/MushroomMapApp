using FluentAssertions;
using MediatR;
using Moq;
using MushroomMap.UnitTests.Common.Database;
using MushroomMapApp.Domain.Data;
using MushroomMapApp.Domain.Entities;
using MushroomMapApp.Domain.Enums;
using MushroomMapApp.Domain.Exceptions;
using MushroomMapApp.Features.Events;
using MushroomMapApp.Features.Users.CreateNewVerificationToken;
using Xunit;

namespace MushroomMap.UnitTests.Features.Users;

public class CreateNewVerificationTokenCommandHandlerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly CreateNewVerificationTokenCommandHandler _handler;

    public CreateNewVerificationTokenCommandHandlerTests()
    {
        _context = TestDbContextFactory.Create();
        _mediatorMock = new Mock<IMediator>();
        _handler = new CreateNewVerificationTokenCommandHandler(_context, _mediatorMock.Object);
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task Handle_CreatesActivationTokenAndPublishesEvent_WhenUserExists()
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
        await _context.SaveChangesAsync();

        var command = new CreateNewVerificationTokenCommand(
            new CreateNewVerificationTokenRequest(Email: user.Email));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().Be(Unit.Value);

        var token = _context.Tokens.Should().ContainSingle().Which;
        token.UserId.Should().Be(user.Id);
        token.TokenType.Should().Be(TokenType.ActivationToken);
        token.TokenData.Should().NotBeNullOrEmpty();
        token.ExpireDateTime.Should().BeAfter(DateTime.UtcNow);

        _mediatorMock.Verify(
            x => x.Publish(
                It.Is<UserRegisteredEvent>(e =>
                    e.Email == user.Email &&
                    e.VerificationCode == token.TokenData),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_Throws_WhenUserDoesNotExist()
    {
        var command = new CreateNewVerificationTokenCommand(
            new CreateNewVerificationTokenRequest(Email: "missing@test.com"));

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
        _context.Tokens.Should().BeEmpty();
        _mediatorMock.Verify(
            x => x.Publish(
                It.IsAny<UserRegisteredEvent>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
