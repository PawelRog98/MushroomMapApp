using System.Security.Claims;
using MushroomMapApp.Domain.Interfaces;

namespace MushroomMapApp.Infrastructure.Services;

public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _accessor;
    public CurrentUser(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    public long? UserId
    {
        get
        {
            var value = _accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return long.TryParse(value, out var id)  ? id : null;
        }
    }
}
