using FluentAssertions;
using MediatR;
using MushroomMap.UnitTests.Common.Database;
using MushroomMapApp.Domain.Data;
using MushroomMapApp.Domain.Entities;
using MushroomMapApp.Domain.Enums;
using MushroomMapApp.Features.Users.Unsuspend;
using Xunit;

namespace MushroomMap.UnitTests.Features.Users;

public class UnsuspendExpiredSuspensionsCommandHandlerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly UnsuspendExpiredSuspensionsCommandHandler _handler;
    private readonly Suspension _expiredActive;
    private readonly Suspension _runningActive;
    private readonly Suspension _alreadyLifted;
    private readonly Suspension _withoutEndDate;

    public UnsuspendExpiredSuspensionsCommandHandlerTests()
    {
        _context = TestDbContextFactory.Create();
        _handler = new UnsuspendExpiredSuspensionsCommandHandler(_context);

        var user = new User
        {
            PublicId = Guid.NewGuid(),
            Email = "test@test.com",
            PublicNick = "test",
            FirstName = "test",
            LastName = "test",
            PasswordHash = "hash"
        };

        var suspendedBy = new User
        {
            PublicId = Guid.NewGuid(),
            Email = "admin@test.com",
            PublicNick = "admin",
            FirstName = "test",
            LastName = "test",
            PasswordHash = "hash"
        };

        _context.Users.AddRange(user, suspendedBy);

        _expiredActive = new Suspension
        {
            User = user,
            SuspendedBy = suspendedBy,
            StartDate = DateTime.UtcNow.AddDays(-10),
            EndDate = DateTime.UtcNow.AddDays(-1),
            Reason = "Expired",
            Status = SuspensionStatusEnum.Active
        };

        _runningActive = new Suspension
        {
            User = user,
            SuspendedBy = suspendedBy,
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddDays(5),
            Reason = "Running",
            Status = SuspensionStatusEnum.Active
        };

        _alreadyLifted = new Suspension
        {
            User = user,
            SuspendedBy = suspendedBy,
            StartDate = DateTime.UtcNow.AddDays(-10),
            EndDate = DateTime.UtcNow.AddDays(-2),
            Reason = "Lifted",
            Status = SuspensionStatusEnum.Lifted
        };

        _withoutEndDate = new Suspension
        {
            User = user,
            SuspendedBy = suspendedBy,
            StartDate = DateTime.UtcNow.AddDays(-10),
            EndDate = null,
            Reason = "No end date",
            Status = SuspensionStatusEnum.Active
        };

        _context.Suspensions.AddRange(
            _expiredActive,
            _runningActive,
            _alreadyLifted,
            _withoutEndDate);
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task Handle_ExpiresOnlyEndedActiveSuspensions()
    {
        await _context.SaveChangesAsync();

        var command = new UnsuspendExpiredSuspensionsCommand();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().Be(Unit.Value);

        _expiredActive.Status.Should().Be(SuspensionStatusEnum.Expired);
        _runningActive.Status.Should().Be(SuspensionStatusEnum.Active);
        _alreadyLifted.Status.Should().Be(SuspensionStatusEnum.Lifted);
        _withoutEndDate.Status.Should().Be(SuspensionStatusEnum.Active);
    }

    [Fact]
    public async Task Handle_DoesNothing_WhenThereAreNoSuspensions()
    {
        await _context.SaveChangesAsync();
        _context.Suspensions.RemoveRange(_context.Suspensions.ToList());
        await _context.SaveChangesAsync();

        var command = new UnsuspendExpiredSuspensionsCommand();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().Be(Unit.Value);
        _context.Suspensions.Should().BeEmpty();
    }
}
