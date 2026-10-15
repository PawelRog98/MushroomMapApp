using FluentValidation;
using MushroomMapApp.Features.Common.Pagination;

namespace MushroomMapApp.Features.Users.GetAllUsers;

public class GetAllUsersQueryValidator : AbstractValidator<GetAllUsersQuery>
{
    public GetAllUsersQueryValidator(PaginationRequestValidator pagingValidator)
    {
        RuleFor(x => x.Paging).NotNull();
        RuleFor(x => x.Sorting).NotNull();

        When(x => x.Paging is not null, () =>
            RuleFor(x => x.Paging!).SetValidator(pagingValidator));

        When(x => x.Sorting is not null, () =>
        {
            RuleFor(x => x.Sorting!.SortBy)
                .Must(sortBy => string.IsNullOrWhiteSpace(sortBy)
                    || UserSortMap.AllowedSortKeys.Contains(sortBy.Trim().ToLowerInvariant()))
                .WithMessage("sortBy must be one of: nick, firstname, lastname, email");

            RuleFor(x => x.Sorting!.SortDir)
                .Must(sortDir => sortDir is null
                    || sortDir.Equals("asc", StringComparison.OrdinalIgnoreCase)
                    || sortDir.Equals("desc", StringComparison.OrdinalIgnoreCase))
                .WithMessage("sortDir must be 'asc' or 'desc'");
        });

        When(x => x.Filters is not null, () =>
        {
            RuleFor(x => x.Filters!.Search)
                .MaximumLength(100)
                .When(search => search is not null)
                .WithMessage("Search must be at most 100 characters");
        });
    }
}
