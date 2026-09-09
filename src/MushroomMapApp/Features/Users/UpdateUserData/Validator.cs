using FluentValidation;

namespace MushroomMapApp.Features.Users.UpdateUserData;

public class Validator : AbstractValidator<UpdateUserDataRequest>
{
    public Validator()
    {
        RuleFor(x => x.PublicNick).NotEmpty().MaximumLength(128);
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(128);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(128);
        RuleFor(x => x.DateOfBirth).LessThanOrEqualTo(DateTime.UtcNow);
        RuleFor(x => x.AccountInfo).MaximumLength(2048);
    }
}
