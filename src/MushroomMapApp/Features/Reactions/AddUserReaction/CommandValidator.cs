using FluentValidation;

namespace MushroomMapApp.Features.Reactions.AddUserReaction;

public class CommandValidator : AbstractValidator<AddLocationReactionCommand>
{
    public CommandValidator(IValidator<AddReactionRequest> requestValidator)
    {
        RuleFor(x => x.request).NotNull().SetValidator(requestValidator);
        RuleFor(x => x.userId).GreaterThan(0);
    }
}
