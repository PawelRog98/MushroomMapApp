using FluentAssertions;
using MushroomMapApp.Features.Common.Pagination;
using Xunit;

namespace MushroomMap.UnitTests.Features.Common;

public class SortRequestTests
{
    [Fact]
    public void IsDescending_ReturnsFalse_WhenSortDirIsNull()
    {
        var request = new SortRequest("nick", null);

        request.IsDescending.Should().BeFalse();
    }

    [Fact]
    public void IsDescending_ReturnsFalse_WhenSortDirIsAsc()
    {
        var request = new SortRequest("nick", "asc");

        request.IsDescending.Should().BeFalse();
    }

    [Fact]
    public void IsDescending_ReturnsTrue_WhenSortDirIsDesc()
    {
        var request = new SortRequest("nick", "desc");

        request.IsDescending.Should().BeTrue();
    }

    [Fact]
    public void IsDescending_ReturnsTrue_WhenSortDirIsDesc_CaseInsensitive()
    {
        var request = new SortRequest("nick", "DESC");

        request.IsDescending.Should().BeTrue();
    }

    [Fact]
    public void IsDescending_ReturnsTrue_WhenSortDirIsDesc_MixedCase()
    {
        var request = new SortRequest("nick", "Desc");

        request.IsDescending.Should().BeTrue();
    }

    [Fact]
    public void DefaultConstructor_SetsDefaults()
    {
        var request = new SortRequest();

        request.SortBy.Should().BeNull();
        request.SortDir.Should().BeNull();
        request.IsDescending.Should().BeFalse();
    }
}
