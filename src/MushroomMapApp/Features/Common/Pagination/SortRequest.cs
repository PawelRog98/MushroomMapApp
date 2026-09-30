namespace MushroomMapApp.Features.Common.Pagination;

public record SortRequest(string? SortBy = null, string? SortDir = null)
{
    public bool IsDescending => string.Equals(SortDir, "desc", StringComparison.OrdinalIgnoreCase);
}
