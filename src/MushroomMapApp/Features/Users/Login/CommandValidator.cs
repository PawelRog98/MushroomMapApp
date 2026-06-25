using FluentValidation;

namespace MushroomMapApp.Features.Users.Login;

public class CommandValidator : AbstractValidator<Command>
{
    public CommandValidator(IValidator<LoginRequest> requestValidator)
    {
        RuleFor(x => x.Login).NotNull().SetValidator(requestValidator);
    }
}
