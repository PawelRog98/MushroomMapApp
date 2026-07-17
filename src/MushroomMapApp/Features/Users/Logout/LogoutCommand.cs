using MediatR;
using MushroomMapApp.Domain.Interfaces;

namespace MushroomMapApp.Features.Users.Logout;

public record Command(long UserId) : IRequest;

public class Handler : IRequestHandler<Command>
{
    private readonly IAuthService _authService;

    public Handler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task Handle(Command command, CancellationToken cancellationToken)
    {
        await _authService.RevokeRefreshTokens(command.UserId, cancellationToken);
    }
}
