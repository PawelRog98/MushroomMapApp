using FluentAssertions;
using FluentValidation.TestHelper;
using MushroomMapApp.Features.Common.Pagination;
using Xunit;

namespace MushroomMap.UnitTests.Features.Common;

public class PaginationRequestValidatorTests
{
    private readonly PaginationRequestValidator _validator = new();

    [Fact]
    public void Validate_DefaultRequest_HasNoErrors()
    {
        var result = _validator.TestValidate(new PaginationRequest());

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(50)]
    public void Validate_ValidPage_Passes(int page)
    {
        var result = _validator.TestValidate(new PaginationRequest(page, 20));

        result.ShouldNotHaveValidationErrorFor(x => x.Page);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Validate_PageBelowOne_HasError(int page)
    {
        var result = _validator.TestValidate(new PaginationRequest(page, 20));

        result.ShouldHaveValidationErrorFor(x => x.Page);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(50)]
    public void Validate_ValidPageSize_Passes(int pageSize)
    {
        var result = _validator.TestValidate(new PaginationRequest(1, pageSize));

        result.ShouldNotHaveValidationErrorFor(x => x.PageSize);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    [InlineData(1000)]
    public void Validate_PageSizeOutOfRange_HasError(int pageSize)
    {
        var result = _validator.TestValidate(new PaginationRequest(1, pageSize));

        result.ShouldHaveValidationErrorFor(x => x.PageSize);
    }
}
