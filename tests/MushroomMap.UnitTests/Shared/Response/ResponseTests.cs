using FluentAssertions;
using MushroomMapApp.Shared.Response;
using Xunit;

namespace MushroomMap.UnitTests.Shared.Response;

public class ResponseTests
{
    [Fact]
    public void Constructor_WithData_SetsData()
    {
        var response = new Response<string>("test");

        response.Data.Should().Be("test");
    }

    [Fact]
    public void Constructor_WithAllParameters_SetsAllProperties()
    {
        var metaData = new { total = 42 };
        var response = new Response<string>("data", true, new[] { "error" }, metaData, "message");

        response.Data.Should().Be("data");
        response.Success.Should().BeTrue();
        response.Errors.Should().BeEquivalentTo("error");
        Assert.Same(metaData, response.MetaData);
        response.Message.Should().Be("message");
    }

    [Fact]
    public void Constructor_WithMessageAndErrors_SetsProperties()
    {
        var response = new Response<string>("message", new[] { "error1", "error2" });

        response.Message.Should().Be("message");
        response.Errors.Should().BeEquivalentTo("error1", "error2");
    }

    [Fact]
    public void DefaultConstructor_SetsDefaults()
    {
        var response = new Response<string>();

        response.Data.Should().BeNull();
        response.Success.Should().BeFalse();
        response.Errors.Should().BeNull();
        Assert.Null(response.MetaData);
        response.Message.Should().Be(string.Empty);
    }

    [Fact]
    public void Success_ConstructorWithData_SetsSuccess()
    {
        var response = new Response<string>("data");

        response.Success.Should().BeFalse();
    }
}
