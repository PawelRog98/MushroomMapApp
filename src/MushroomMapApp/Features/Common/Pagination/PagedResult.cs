namespace MushroomMapApp.Features.Common.Pagination;

public record PagedResult<T>(IReadOnlyList<T> items, int TotalCount, int Page, int PageSize) where T : class
{
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;
}
