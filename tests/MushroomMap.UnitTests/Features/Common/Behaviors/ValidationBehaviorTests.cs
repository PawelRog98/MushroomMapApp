using FluentAssertions;
using FluentValidation;
using MushroomMapApp.Features.Common.Behaviors;
using Xunit;

namespace MushroomMap.UnitTests.Features.Common.Behaviors;

public class ValidationBehaviorTests
{
    private sealed class TestRequest
    {
        public string? Value { get; set; }
    }

    private sealed record TestResponse(string Value);

    private sealed class NotEmptyValidator : AbstractValidator<TestRequest>
    {
        public NotEmptyValidator()
        {
            RuleFor(x => x.Value).NotEmpty();
        }
    }

    private sealed class MaxLengthValidator : AbstractValidator<TestRequest>
    {
        public MaxLengthValidator()
        {
            RuleFor(x => x.Value).MaximumLength(2);
        }
    }

    private sealed class ExpectedValueValidator : AbstractValidator<TestRequest>
    {
        public ExpectedValueValidator()
        {
            RuleFor(x => x.Value)
                .Must(v => v == "expected")
                .WithMessage("'Value' is incorrect.");
        }
    }

    private readonly TestRequest _request = new();
    private readonly TestResponse _response = new("handled");
    private bool _nextCalled;

    private Task<TestResponse> Next(CancellationToken cancellationToken)
    {
        _nextCalled = true;
        return Task.FromResult(_response);
    }

    [Fact]
    public async Task Handle_CallsNext_WhenThereAreNoValidators()
    {
        var behavior = new ValidationBehavior<TestRequest, TestResponse>([]);

        var result = await behavior.Handle(_request, Next, CancellationToken.None);

        _nextCalled.Should().BeTrue();
        result.Should().BeSameAs(_response);
    }

    [Fact]
    public async Task Handle_CallsNext_WhenValidationSucceeds()
    {
        var behavior = new ValidationBehavior<TestRequest, TestResponse>([new NotEmptyValidator()]);

        _request.Value = "valid";

        var result = await behavior.Handle(_request, Next, CancellationToken.None);

        _nextCalled.Should().BeTrue();
        result.Should().BeSameAs(_response);
    }

    [Fact]
    public async Task Handle_ThrowsValidationException_WhenValidationFails()
    {
        var behavior = new ValidationBehavior<TestRequest, TestResponse>([new NotEmptyValidator()]);

        _request.Value = string.Empty;

        var act = () => behavior.Handle(_request, Next, CancellationToken.None);

        var exception = await act.Should().ThrowAsync<ValidationException>();
        exception.Which.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be("'Value' must not be empty.");
        _nextCalled.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_CollectsFailuresFromAllValidators_WhenMultipleValidatorsFail()
    {
        var behavior = new ValidationBehavior<TestRequest, TestResponse>(
        [
            new MaxLengthValidator(),
            new ExpectedValueValidator()
        ]);

        _request.Value = "abc";

        var act = () => behavior.Handle(_request, Next, CancellationToken.None);

        var exception = await act.Should().ThrowAsync<ValidationException>();
        exception.Which.Errors
            .Select(x => x.ErrorMessage)
            .Should().BeEquivalentTo(
                "The length of 'Value' must be 2 characters or fewer. You entered 3 characters.",
                "'Value' is incorrect.");
        _nextCalled.Should().BeFalse();
    }
}
