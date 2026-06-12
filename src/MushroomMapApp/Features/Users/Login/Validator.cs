using FluentValidation;

namespace MushroomMapApp.Features.Users.Login;

public class Validator : AbstractValidator<LoginRequest>
{
    public Validator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}
