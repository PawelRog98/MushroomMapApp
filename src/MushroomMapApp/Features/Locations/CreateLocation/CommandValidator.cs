using FluentValidation;

namespace MushroomMapApp.Features.Locations.CreateLocation;

public class CommandValidator : AbstractValidator<CreateLocationCommand>
{
    public CommandValidator(IValidator<CreateLocationRequest> requestValidator)
    {
        RuleFor(x => x.request).NotNull().SetValidator(requestValidator);
    }
}
