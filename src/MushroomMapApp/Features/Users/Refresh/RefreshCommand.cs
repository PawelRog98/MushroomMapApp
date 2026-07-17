using MediatR;
using MushroomMapApp.Domain.Interfaces;
using MushroomMapApp.Features.Users.Login;

namespace MushroomMapApp.Features.Users.Refresh;

public record RefreshRequest(string RefreshToken);

public record Command(RefreshRequest Refresh) : IRequest<AuthTokenDto>;

public class Handler : IRequestHandler<Command, AuthTokenDto>
{
    private readonly IAuthService _authService;

    public Handler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<AuthTokenDto> Handle(Command command, CancellationToken cancellationToken)
    {
        var token = await _authService.RefreshToken(command.Refresh.RefreshToken, cancellationToken);

        return new AuthTokenDto
        {
            AccessToken = token.AccessToken,
            RefreshToken = token.RefreshToken,
            UserNick = token.UserNick
        };
    }
}
