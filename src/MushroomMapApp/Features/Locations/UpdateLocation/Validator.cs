using FluentValidation;

namespace MushroomMapApp.Features.Locations.UpdateLocation;

public class Validator : AbstractValidator<UpdateLocationRequest>
{
    public Validator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(512);
        RuleFor(x => x.Text).NotEmpty().MaximumLength(4096);
    }
}
