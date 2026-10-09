using FluentAssertions;
using FluentValidation.TestHelper;
using MushroomMapApp.Features.Reactions.AddUserReaction;
using Xunit;

namespace MushroomMap.UnitTests.Features.Reactions;

public class AddLocationReactionValidatorTests
{
    private readonly Validator _validator = new();

    private static AddReactionRequest ValidRequest() => new(Guid.NewGuid(), Guid.NewGuid());

    [Fact]
    public void Validate_ValidRequest_HasNoErrors()
    {
        var result = _validator.TestValidate(ValidRequest());

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyLocationPublicId_HasError()
    {
        var result = _validator.TestValidate(new AddReactionRequest(Guid.Empty, Guid.NewGuid()));

        result.ShouldHaveValidationErrorFor(x => x.locationPublicId);
    }

    [Fact]
    public void Validate_EmptyReactionTypePublicId_HasError()
    {
        var result = _validator.TestValidate(new AddReactionRequest(Guid.NewGuid(), Guid.Empty));

        result.ShouldHaveValidationErrorFor(x => x.reactionTypePublicId);
    }
}
