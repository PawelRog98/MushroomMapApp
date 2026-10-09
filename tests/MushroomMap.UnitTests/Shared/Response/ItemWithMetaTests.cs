using FluentAssertions;
using MushroomMapApp.Shared.Response;
using Xunit;

namespace MushroomMap.UnitTests.Shared.Response;

public class ItemWithMetaTests
{
    [Fact]
    public void Properties_CanBeSet()
    {
        var meta = new { id = 1 };
        var item = new ItemWithMeta<string>
        {
            Data = "test",
            Meta = meta
        };

        item.Data.Should().Be("test");
        Assert.Same(meta, item.Meta);
    }

    [Fact]
    public void Properties_DefaultToNull()
    {
        var item = new ItemWithMeta<string>();

        item.Data.Should().BeNull();
        Assert.Null(item.Meta);
    }
}
