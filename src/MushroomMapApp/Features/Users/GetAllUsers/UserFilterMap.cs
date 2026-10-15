using MushroomMapApp.Domain.Entities;
using MushroomMapApp.Features.Common.Pagination;

namespace MushroomMapApp.Features.Users.GetAllUsers;

public static class UserFilterMap
{
    public static IQueryable<User> ApplyUserFilters(this IQueryable<User> query, FiltersRequest? filters)
    {
        var search = filters?.Search?.Trim();

        if (string.IsNullOrEmpty(search))
            return query;

        var term = search.ToLower();
        return query.Where(x =>
            x.FirstName.ToLower().Contains(term) ||
            x.LastName.ToLower().Contains(term) ||
            x.Email.ToLower().Contains(term) ||
            x.PublicNick.ToLower().Contains(term));
    }
}
