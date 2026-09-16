using FluentValidation;

namespace MushroomMapApp.Features.Users.UpdateAvatar;

public class CommandValidator : AbstractValidator<UpdateAvatarCommand>
{
    public CommandValidator(IValidator<UpdateAvatarRequest> requestValidator)
    {
        RuleFor(x => x.Request).NotNull().SetValidator(requestValidator);
        RuleFor(x => x.UserId).GreaterThan(0);
    }
}
