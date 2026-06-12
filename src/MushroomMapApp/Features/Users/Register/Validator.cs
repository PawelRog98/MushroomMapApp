using FluentValidation;

namespace MushroomMapApp.Features.Users.Register;

public class Validator : AbstractValidator<RegisterRequest>
{
    public Validator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.PublicNick).NotEmpty();
        RuleFor(x => x.FirstName).NotEmpty();
        RuleFor(x => x.LastName).NotEmpty();
        RuleFor(x => x.Password).MinimumLength(8);
        RuleFor(x => x.ConfirmPassword).Equal(y => y.Password);
        RuleFor(x => x.DateOfBirth).NotEmpty();
    }
}
