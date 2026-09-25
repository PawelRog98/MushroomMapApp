using FluentAssertions;
using MediatR;
using MushroomMap.UnitTests.Common.Database;
using MushroomMapApp.Domain.Data;
using MushroomMapApp.Domain.Entities;
using MushroomMapApp.Domain.Enums;
using MushroomMapApp.Domain.Exceptions;
using MushroomMapApp.Features.Users.Unsuspend;
using Xunit;

namespace MushroomMap.UnitTests.Features.Users;

public class UnsuspendUserCommandHandlerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly UnsuspendUserCommandHandler _handler;
    private readonly User _user;
    private readonly Suspension _activeSuspension;
    private readonly Suspension _liftedSuspension;
    private readonly Suspension _expiredSuspension;
    private readonly Suspension _otherUserActiveSuspension;

    public UnsuspendUserCommandHandlerTests()
    {
        _context = TestDbContextFactory.Create();
        _handler = new UnsuspendUserCommandHandler(_context);

        _user = new User
        {
            PublicId = Guid.NewGuid(),
            Email = "test@test.com",
            PublicNick = "test",
            FirstName = "test",
            LastName = "test",
            PasswordHash = "hash"
        };

        var otherUser = new User
        {
            PublicId = Guid.NewGuid(),
            Email = "other@test.com",
            PublicNick = "other",
            FirstName = "test",
            LastName = "test",
            PasswordHash = "hash"
        };

        _context.Users.AddRange(_user, otherUser);

        _activeSuspension = new Suspension
        {
            User = _user,
            SuspendedBy = otherUser,
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddDays(5),
            Reason = "Active",
            Status = SuspensionStatusEnum.Active
        };

        _liftedSuspension = new Suspension
        {
            User = _user,
            SuspendedBy = otherUser,
            StartDate = DateTime.UtcNow.AddDays(-10),
            EndDate = DateTime.UtcNow.AddDays(-5),
            Reason = "Lifted",
            Status = SuspensionStatusEnum.Lifted
        };

        _expiredSuspension = new Suspension
        {
            User = _user,
            SuspendedBy = otherUser,
            StartDate = DateTime.UtcNow.AddDays(-20),
            EndDate = DateTime.UtcNow.AddDays(-15),
            Reason = "Expired",
            Status = SuspensionStatusEnum.Expired
        };

        _otherUserActiveSuspension = new Suspension
        {
            User = otherUser,
            SuspendedBy = otherUser,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(3),
            Reason = "Other user",
            Status = SuspensionStatusEnum.Active
        };

        _context.Suspensions.AddRange(
            _activeSuspension,
            _liftedSuspension,
            _expiredSuspension,
            _otherUserActiveSuspension);
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task Handle_LiftsActiveSuspensions_WhenUserExists()
    {
        await _context.SaveChangesAsync();

        var command = new UnsuspendUserCommand(
            new UnsuspendUserRequest(UserPublicId: _user.PublicId));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().Be(Unit.Value);

        _activeSuspension.Status.Should().Be(SuspensionStatusEnum.Lifted);
        _liftedSuspension.Status.Should().Be(SuspensionStatusEnum.Lifted);
        _expiredSuspension.Status.Should().Be(SuspensionStatusEnum.Expired);
        _otherUserActiveSuspension.Status.Should().Be(SuspensionStatusEnum.Active);
    }

    [Fact]
    public async Task Handle_Throws_WhenUserDoesNotExist()
    {
        await _context.SaveChangesAsync();

        var command = new UnsuspendUserCommand(
            new UnsuspendUserRequest(UserPublicId: Guid.NewGuid()));

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>();
        _activeSuspension.Status.Should().Be(SuspensionStatusEnum.Active);
    }
}
