using FluentAssertions;
using MediatR;
using Moq;
using MushroomMapApp.Features.Jobs.Recurring;
using MushroomMapApp.Features.Users.Unsuspend;
using Xunit;

namespace MushroomMap.UnitTests.Features.Jobs;

public class UnsuspendExpiredSuspensionsJobTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly UnsuspendExpiredSuspensionsJob _job;

    public UnsuspendExpiredSuspensionsJobTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _job = new UnsuspendExpiredSuspensionsJob(_mediatorMock.Object);
    }

    [Fact]
    public async Task ExecuteJob_SendsUnsuspendExpiredSuspensionsCommand()
    {
        await _job.ExecuteJob(CancellationToken.None);

        _mediatorMock.Verify(
            x => x.Send(
                It.Is<UnsuspendExpiredSuspensionsCommand>(c => c != null),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteJob_PassesCancellationToken()
    {
        var cts = new CancellationTokenSource();
        var token = cts.Token;

        await _job.ExecuteJob(token);

        _mediatorMock.Verify(
            x => x.Send(
                It.IsAny<UnsuspendExpiredSuspensionsCommand>(),
                token),
            Times.Once);
    }
}
