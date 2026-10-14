using FluentValidation;

namespace MushroomMapApp.Features.Users.CreateUserLocation;

public class CommandValidator : AbstractValidator<CreateUserLocationCommand>
{
    public CommandValidator(IValidator<CreateUserLocationRequest> requestValidator)
    {
        RuleFor(x => x.Request).NotNull().SetValidator(requestValidator);
        RuleFor(x => x.UserId).GreaterThan(0);
    }
}
