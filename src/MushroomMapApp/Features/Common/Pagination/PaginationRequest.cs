namespace MushroomMapApp.Features.Common.Pagination;

public record PaginationRequest(int Page = 1, int PageSize = 20);
