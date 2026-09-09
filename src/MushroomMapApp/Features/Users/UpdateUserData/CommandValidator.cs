using FluentValidation;

namespace MushroomMapApp.Features.Users.UpdateUserData;

public class CommandValidator : AbstractValidator<UpdateUserDataUpdateCommand>
{
    public CommandValidator(IValidator<UpdateUserDataRequest> requestValidator)
    {
        RuleFor(x => x.UpdateUserDataRequest).NotNull().SetValidator(requestValidator);
        RuleFor(x => x.UserId).GreaterThan(0);
    }
}
