using FluentAssertions;
using MediatR;
using MushroomMap.UnitTests.Common.Database;
using MushroomMapApp.Domain.Data;
using MushroomMapApp.Domain.Entities;
using MushroomMapApp.Features.Users.UpdateUserData;
using Xunit;

namespace MushroomMap.UnitTests.Features.Users;

public class UserDataUpdateCommandHandlerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly UserDataUpdateCommandHandler _handler;
    private readonly User _user;

    public UserDataUpdateCommandHandlerTests()
    {
        _context = TestDbContextFactory.Create();
        _handler = new UserDataUpdateCommandHandler(_context);

        _user = new User
        {
            PublicId = Guid.NewGuid(),
            Email = "test@test.com",
            PublicNick = "old-nick",
            FirstName = "Old",
            LastName = "Name",
            DateOfBirth = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            PasswordHash = "hash",
            AccountInfo = "old info"
        };

        _context.Users.Add(_user);
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task Handle_UpdatesUserData_WhenUserExists()
    {
        await _context.SaveChangesAsync();

        var request = new UpdateUserDataRequest(
            PublicNick: "new-nick",
            FirstName: "New",
            LastName: "Surname",
            DateOfBirth: new DateTime(1995, 5, 10, 0, 0, 0, DateTimeKind.Unspecified),
            AccountInfo: "new info");

        var command = new UpdateUserDataUpdateCommand(
            UpdateUserDataRequest: request,
            UserId: _user.Id);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().Be(Unit.Value);

        var user = _context.Users.Single();
        user.PublicNick.Should().Be("new-nick");
        user.FirstName.Should().Be("New");
        user.LastName.Should().Be("Surname");
        user.AccountInfo.Should().Be("new info");
        user.DateOfBirth.Should().Be(new DateTime(1995, 5, 10, 0, 0, 0, DateTimeKind.Utc));
        user.DateOfBirth.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public async Task Handle_KeepsUnrelatedFields_WhenUserExists()
    {
        await _context.SaveChangesAsync();

        var request = new UpdateUserDataRequest(
            PublicNick: "new-nick",
            FirstName: "New",
            LastName: "Surname",
            DateOfBirth: new DateTime(1995, 5, 10),
            AccountInfo: "new info");

        var command = new UpdateUserDataUpdateCommand(
            UpdateUserDataRequest: request,
            UserId: _user.Id);

        await _handler.Handle(command, CancellationToken.None);

        var user = _context.Users.Single();
        user.Email.Should().Be("test@test.com");
        user.PasswordHash.Should().Be("hash");
        user.IsEmailConfirmed.Should().BeFalse();
        user.PublicId.Should().NotBe(Guid.Empty);
    }
}
