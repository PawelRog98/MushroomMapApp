using FluentValidation;

namespace MushroomMapApp.Features.Users.Register;

public class CommandValidator : AbstractValidator<Command>
{
    public CommandValidator(IValidator<RegisterRequest> requestValidator)
    {
        RuleFor(x => x.Register).NotNull().SetValidator(requestValidator);
    }
}
