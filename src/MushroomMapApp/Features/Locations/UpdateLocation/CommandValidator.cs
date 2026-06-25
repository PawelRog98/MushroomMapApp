using FluentValidation;

namespace MushroomMapApp.Features.Locations.UpdateLocation;

public class CommandValidator : AbstractValidator<UpdateLocationCommand>
{
    public CommandValidator(IValidator<UpdateLocationRequest> requestValidator)
    {
        RuleFor(x => x.Request).NotNull().SetValidator(requestValidator);
        RuleFor(x => x.PublicId).NotEmpty();
    }
}
