using FluentAssertions;
using FluentValidation.TestHelper;
using MushroomMapApp.Features.Common.Pagination;
using MushroomMapApp.Features.Users.GetAllUsers;
using Xunit;

namespace MushroomMap.UnitTests.Features.Users;

public class GetAllUsersQueryValidatorTests
{
    private readonly GetAllUsersQueryValidator _validator = new(new PaginationRequestValidator());

    private static GetAllUsersQuery Query(PaginationRequest? paging = null, SortRequest? sorting = null)
        => new(paging ?? new PaginationRequest(), sorting ?? new SortRequest());

    [Fact]
    public void Validate_DefaultRequest_HasNoErrors()
    {
        var result = _validator.TestValidate(Query());

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_PageBelowOne_HasError(int page)
    {
        var result = _validator.TestValidate(Query(new PaginationRequest(page, 20)));

        result.ShouldHaveValidationErrorFor(x => x.Paging.Page);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void Validate_PageSizeOutOfRange_HasError(int pageSize)
    {
        var result = _validator.TestValidate(Query(new PaginationRequest(1, pageSize)));

        result.ShouldHaveValidationErrorFor(x => x.Paging.PageSize);
    }

    [Theory]
    [InlineData("nick")]
    [InlineData("firstname")]
    [InlineData("lastname")]
    [InlineData("email")]
    [InlineData(" EMAIL ")]
    public void Validate_WhitelistedSortKey_Passes(string sortBy)
    {
        var result = _validator.TestValidate(Query(sorting: new SortRequest(sortBy, "asc")));

        result.ShouldNotHaveValidationErrorFor(x => x.Sorting!.SortBy);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Validate_MissingSortKey_Passes(string? sortBy)
    {
        var result = _validator.TestValidate(Query(sorting: new SortRequest(sortBy, null)));

        result.ShouldNotHaveValidationErrorFor(x => x.Sorting!.SortBy);
    }

    [Theory]
    [InlineData("password")]
    [InlineData("roleName")]
    [InlineData("id")]
    public void Validate_UnknownSortKey_HasError(string sortBy)
    {
        var result = _validator.TestValidate(Query(sorting: new SortRequest(sortBy, "asc")));

        result.ShouldHaveValidationErrorFor(x => x.Sorting!.SortBy)
            .WithErrorMessage("sortBy must be one of: nick, firstname, lastname, email");
    }

    [Theory]
    [InlineData("asc")]
    [InlineData("DESC")]
    [InlineData(null)]
    public void Validate_ValidOrMissingSortDir_Passes(string? sortDir)
    {
        var result = _validator.TestValidate(Query(sorting: new SortRequest("nick", sortDir)));

        result.ShouldNotHaveValidationErrorFor(x => x.Sorting!.SortDir);
    }

    [Theory]
    [InlineData("upside")]
    [InlineData("descending")]
    public void Validate_InvalidSortDir_HasError(string sortDir)
    {
        var result = _validator.TestValidate(Query(sorting: new SortRequest("nick", sortDir)));

        result.ShouldHaveValidationErrorFor(x => x.Sorting!.SortDir)
            .WithErrorMessage("sortDir must be 'asc' or 'desc'");
    }

    [Fact]
    public void Validate_NullSorting_HasError()
    {
        var result = _validator.TestValidate(new GetAllUsersQuery(new PaginationRequest(), null!));

        result.ShouldHaveValidationErrorFor(x => x.Sorting);
    }
}
