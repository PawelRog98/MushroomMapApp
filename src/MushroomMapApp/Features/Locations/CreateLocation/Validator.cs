using FluentValidation;

namespace MushroomMapApp.Features.Locations.CreateLocation;

public class Validator : AbstractValidator<CreateLocationRequest>
{
    public Validator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(512);
        RuleFor(x => x.Text).NotEmpty().MaximumLength(4096);
        RuleFor(x => x.Lat).NotNull();
        RuleFor(x => x.Lng).NotNull();
    }
}
