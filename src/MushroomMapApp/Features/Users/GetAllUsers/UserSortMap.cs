using System.Linq.Expressions;
using FluentValidation;
using FluentValidation.Results;
using MushroomMapApp.Domain.Entities;
using MushroomMapApp.Features.Common.Pagination;

namespace MushroomMapApp.Features.Users.GetAllUsers;

public static class UserSortMap
{
    public static readonly string[] AllowedSortKeys = ["nick", "firstname", "lastname", "email"];

    public static IOrderedQueryable<User> SortUsers(this IQueryable<User> query, SortRequest sortRequest)
    {
        IOrderedQueryable<User> ordered = sortRequest.SortBy?.Trim().ToLowerInvariant() switch
        {
            null or "" or "nick" => Initialize(query, x => x.PublicNick, sortRequest.IsDescending),
            "firstname" => Initialize(query, x => x.FirstName, sortRequest.IsDescending),
            "lastname" => Initialize(query, x => x.LastName, sortRequest.IsDescending),
            "email" => Initialize(query, x => x.Email, sortRequest.IsDescending),
            _ => throw new ValidationException(new[]
            {
                new ValidationFailure(nameof(SortRequest.SortBy),
                    "sortBy must be one of: nick, firstname, lastname, email")
            })
        };
        return ordered.ThenBy(x => x.Id);
    }

    private static IOrderedQueryable<User> Initialize<T>(IQueryable<User> query, Expression<Func<User, T>> key, bool desc)
        =>  desc ? query.OrderByDescending(key) : query.OrderBy(key);
}
