using FluentValidation;

namespace MushroomMapApp.Features.Users.CreateUserLocation;

public class Validator : AbstractValidator<CreateUserLocationRequest>
{
    public Validator()
    {
        RuleFor(x => x.Lat).InclusiveBetween(-90, 90);
        RuleFor(x => x.Lng).InclusiveBetween(-180, 180);
    }
}
