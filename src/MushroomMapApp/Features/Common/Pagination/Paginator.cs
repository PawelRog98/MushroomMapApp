using Microsoft.EntityFrameworkCore;

namespace MushroomMapApp.Features.Common.Pagination;

public static class Paginator
{
    public static async Task<PagedResult<T>> PaginateList<T>(
        this IQueryable<T> query,
        PaginationRequest page,
        Func<IQueryable<T>, IOrderedQueryable<T>> orderBy,
        CancellationToken cancellationToken) where T : class
    {
        var totalCount = await query.CountAsync(cancellationToken);

        var items = await orderBy(query)
            .Skip((page.Page - 1) * page.PageSize)
            .Take(page.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<T>(items, totalCount, page.Page, page.PageSize);
    }
}
