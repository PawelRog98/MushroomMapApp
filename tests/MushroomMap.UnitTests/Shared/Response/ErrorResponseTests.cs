using FluentAssertions;
using MushroomMapApp.Shared.Response;
using Xunit;

namespace MushroomMap.UnitTests.Shared.Response;

public class ErrorResponseTests
{
    [Fact]
    public void DefaultConstructor_SetsSuccessFalse()
    {
        var response = new ErrorResponse();

        response.Success.Should().BeFalse();
    }

    [Fact]
    public void Constructor_WithMessage_SetsMessage()
    {
        var response = new ErrorResponse("Error message");

        response.Message.Should().Be("Error message");
        response.Success.Should().BeFalse();
    }

    [Fact]
    public void Constructor_WithMessageAndErrors_SetsProperties()
    {
        var response = new ErrorResponse("Error", new[] { "err1", "err2" });

        response.Message.Should().Be("Error");
        response.Errors.Should().BeEquivalentTo("err1", "err2");
        response.Success.Should().BeFalse();
    }

    [Fact]
    public void Constructor_WithMessageErrorsAndMetaData_SetsProperties()
    {
        var metaData = new { code = 42 };
        var response = new ErrorResponse("Error", new[] { "err" }, metaData);

        response.Message.Should().Be("Error");
        response.Errors.Should().BeEquivalentTo("err");
        Assert.Same(metaData, response.MetaData);
        response.Success.Should().BeFalse();
    }

    [Fact]
    public void Constructor_WithMessageAndMetaData_SetsProperties()
    {
        var metaData = new { code = 42 };
        var response = new ErrorResponse("Error", metaData);

        response.Message.Should().Be("Error");
        Assert.Same(metaData, response.MetaData);
        response.Success.Should().BeFalse();
    }

    [Fact]
    public void InheritsFromResponse()
    {
        var response = new ErrorResponse();

        response.Should().BeAssignableTo<Response<object>>();
    }
}
