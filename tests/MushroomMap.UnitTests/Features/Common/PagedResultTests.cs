using FluentAssertions;
using MushroomMapApp.Features.Common.Pagination;
using Xunit;

namespace MushroomMap.UnitTests.Features.Common;

public class PagedResultTests
{
    [Fact]
    public void TotalPages_ReturnsCorrectValue()
    {
        var result = new PagedResult<string>(new List<string>(), 25, 1, 10);

        result.TotalPages.Should().Be(3);
    }

    [Fact]
    public void TotalPages_ReturnsZero_WhenNoItems()
    {
        var result = new PagedResult<string>(new List<string>(), 0, 1, 10);

        result.TotalPages.Should().Be(0);
    }

    [Fact]
    public void TotalPages_ReturnsOne_WhenExactlyOnePage()
    {
        var result = new PagedResult<string>(new List<string>(), 10, 1, 10);

        result.TotalPages.Should().Be(1);
    }

    [Fact]
    public void HasPrevious_ReturnsFalse_OnFirstPage()
    {
        var result = new PagedResult<string>(new List<string>(), 25, 1, 10);

        result.HasPrevious.Should().BeFalse();
    }

    [Fact]
    public void HasPrevious_ReturnsTrue_NotOnFirstPage()
    {
        var result = new PagedResult<string>(new List<string>(), 25, 2, 10);

        result.HasPrevious.Should().BeTrue();
    }

    [Fact]
    public void HasNext_ReturnsFalse_OnLastPage()
    {
        var result = new PagedResult<string>(new List<string>(), 25, 3, 10);

        result.HasNext.Should().BeFalse();
    }

    [Fact]
    public void HasNext_ReturnsTrue_NotOnLastPage()
    {
        var result = new PagedResult<string>(new List<string>(), 25, 2, 10);

        result.HasNext.Should().BeTrue();
    }

    [Fact]
    public void HasNext_ReturnsFalse_WhenNoItems()
    {
        var result = new PagedResult<string>(new List<string>(), 0, 1, 10);

        result.HasNext.Should().BeFalse();
    }

    [Fact]
    public void Constructor_SetsProperties()
    {
        var items = new List<string> { "a", "b" };
        var result = new PagedResult<string>(items, 100, 2, 20);

        result.items.Should().BeSameAs(items);
        result.TotalCount.Should().Be(100);
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(20);
    }
}
