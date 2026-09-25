namespace MushroomMapApp.Shared.Response;

public class PaginatedResponse<T> : Response<IEnumerable<T>>
{
    public int CurrentPage { get; private set; }
    public int PageSize { get; private set; }
    public int TotalCount { get; private set; }
    public int TotalPages { get; private set; }

    PaginatedResponse(IEnumerable<T> data, int currentPage, int pageSize, int totalCount, int totalPages) : base(data)
    {
        CurrentPage = currentPage;
        PageSize = pageSize;
        TotalCount = totalCount;
        TotalPages = totalPages;
    }
}
