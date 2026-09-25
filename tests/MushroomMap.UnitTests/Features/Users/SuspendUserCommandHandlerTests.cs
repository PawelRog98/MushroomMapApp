using FluentAssertions;
using MediatR;
using MushroomMap.UnitTests.Common.Database;
using MushroomMapApp.Domain.Data;
using MushroomMapApp.Domain.Entities;
using MushroomMapApp.Domain.Enums;
using MushroomMapApp.Domain.Exceptions;
using MushroomMapApp.Features.Users.Suspend;
using Xunit;

namespace MushroomMap.UnitTests.Features.Users;

public class SuspendUserCommandHandlerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly SuspendUserCommandHandler _handler;

    public SuspendUserCommandHandlerTests()
    {
        _context = TestDbContextFactory.Create();
        _handler = new SuspendUserCommandHandler(_context);
    }

    public void Dispose() => _context.Dispose();

    private static User CreateUser(string email)
    {
        return new User
        {
            PublicId = Guid.NewGuid(),
            Email = email,
            PublicNick = email.Split('@')[0],
            FirstName = "test",
            LastName = "test",
            PasswordHash = "hash"
        };
    }

    [Fact]
    public async Task Handle_CreatesActiveSuspension_WhenUserExists()
    {
        var user = CreateUser("test@test.com");
        var currentUserId = CreateUser("admin@test.com");
        _context.Users.AddRange(user, currentUserId);
        await _context.SaveChangesAsync();

        var command = new SuspendUserCommand(
            CurrentUserId: currentUserId.Id,
            Request: new SuspendUserRequest(
                UserPublicId: user.PublicId,
                Days: 7,
                Reason: "Spamming"));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().Be(Unit.Value);

        var suspension = _context.Suspensions.Should().ContainSingle().Which;
        suspension.UserId.Should().Be(user.Id);
        suspension.SuspendedById.Should().Be(currentUserId.Id);
        suspension.Reason.Should().Be("Spamming");
        suspension.Status.Should().Be(SuspensionStatusEnum.Active);
        suspension.StartDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));

        suspension.EndDate.Should().NotBeNull();
        suspension.EndDate!.Value.TimeOfDay.Should().Be(new TimeSpan(0, 23, 59, 59, 999));
        suspension.EndDate.Value.Date.Should().BeOnOrAfter(DateTime.UtcNow.Date.AddDays(7));
        suspension.EndDate.Value.Date.Should().BeOnOrBefore(DateTime.UtcNow.Date.AddDays(8));
    }

    [Fact]
    public async Task Handle_Throws_WhenUserDoesNotExist()
    {
        var currentUserId = CreateUser("admin@test.com");
        _context.Users.Add(currentUserId);
        await _context.SaveChangesAsync();

        var command = new SuspendUserCommand(
            CurrentUserId: currentUserId.Id,
            Request: new SuspendUserRequest(
                UserPublicId: Guid.NewGuid(),
                Days: 7,
                Reason: "Spamming"));

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>();
        _context.Suspensions.Should().BeEmpty();
    }
}
