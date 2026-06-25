using FluentValidation;

namespace MushroomMapApp.Features.Reactions.AddUserReaction;

public class Validator : AbstractValidator<AddReactionRequest>
{
    public Validator()
    {
        RuleFor(x => x.locationPublicId).NotEmpty();
        RuleFor(x => x.reactionTypePublicId).NotEmpty();
    }
}
