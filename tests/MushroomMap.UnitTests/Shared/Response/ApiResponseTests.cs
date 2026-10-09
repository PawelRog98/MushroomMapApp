using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MushroomMapApp.Shared.Response;
using Xunit;

namespace MushroomMap.UnitTests.Shared.Response;

public class ApiResponseTests
{
    [Fact]
    public void Ok_Returns200_WithData()
    {
        var result = ApiResponse.Ok<string>("test data");

        var okResult = result.Should().BeOfType<OkObjectResult>().Which;
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);

        var response = okResult.Value.Should().BeOfType<Response<string>>().Which;
        response.Success.Should().BeTrue();
        response.Data.Should().Be("test data");
        response.Message.Should().Be("Request succeeded");
    }

    [Fact]
    public void Ok_WithCustomMessage_ReturnsMessage()
    {
        var result = ApiResponse.Ok("data", "Custom message");

        var response = ((OkObjectResult)result).Value.Should().BeOfType<Response<string>>().Which;
        response.Message.Should().Be("Custom message");
    }

    [Fact]
    public void Ok_WithMetaData_IncludesMetaData()
    {
        var result = ApiResponse.Ok("data", metaDataAction: meta => meta.total = 42);

        var response = ((OkObjectResult)result).Value.Should().BeOfType<Response<object>>().Which;
        Assert.NotNull((object)response.MetaData);
    }

    [Fact]
    public void Ok_WithoutData_ReturnsNullData()
    {
        var result = ApiResponse.Ok();

        var response = ((OkObjectResult)result).Value.Should().BeOfType<Response<object>>().Which;
        response.Success.Should().BeTrue();
        response.Data.Should().BeNull();
    }

    [Fact]
    public void Ok_WithEnumerableAndItemMetaData_WrapsItems()
    {
        var items = new List<string> { "a", "b" };
        var result = ApiResponse.Ok(items, itemMetaDataAction: (item, meta) => meta.itemName = item);

        var okResult = result.Should().BeOfType<OkObjectResult>().Which;
        var response = okResult.Value.Should().BeOfType<Response<List<ItemWithMeta<object>>>>().Which;
        response.Success.Should().BeTrue();
        response.Data.Should().HaveCount(2);
    }

    [Fact]
    public void NoContent_Returns404()
    {
        var result = ApiResponse.NoContent();

        var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Which;
        notFoundResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);

        var response = notFoundResult.Value.Should().BeOfType<ErrorResponse>().Which;
        response.Success.Should().BeFalse();
    }

    [Fact]
    public void Forbidden_Returns403()
    {
        var result = ApiResponse.Forbidden();

        var objectResult = result.Should().BeOfType<ObjectResult>().Which;
        objectResult.StatusCode.Should().Be(StatusCodes.Status403Forbidden);

        var response = objectResult.Value.Should().BeOfType<ErrorResponse>().Which;
        response.Success.Should().BeFalse();
    }

    [Fact]
    public void BadRequest_Returns400()
    {
        var result = ApiResponse.BadRequest();

        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Which;
        badRequestResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

        var response = badRequestResult.Value.Should().BeOfType<ErrorResponse>().Which;
        response.Success.Should().BeFalse();
    }

    [Fact]
    public void BadRequest_WithErrors_IncludesErrors()
    {
        var errors = new[] { "Error 1", "Error 2" };
        var result = ApiResponse.BadRequest("Bad request", errors);

        var response = ((BadRequestObjectResult)result).Value.Should().BeOfType<ErrorResponse>().Which;
        response.Errors.Should().BeEquivalentTo("Error 1", "Error 2");
    }

    [Fact]
    public void NotFound_Returns404()
    {
        var result = ApiResponse.NotFound();

        var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Which;
        notFoundResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);

        var response = notFoundResult.Value.Should().BeOfType<ErrorResponse>().Which;
        response.Success.Should().BeFalse();
        response.Message.Should().Be("Resource not found");
    }

    [Fact]
    public void InternalServerError_Returns500()
    {
        var result = ApiResponse.InternalServerError();

        var objectResult = result.Should().BeOfType<ObjectResult>().Which;
        objectResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);

        var response = objectResult.Value.Should().BeOfType<ErrorResponse>().Which;
        response.Success.Should().BeFalse();
    }

    [Fact]
    public void InternalServerError_WithErrors_IncludesErrors()
    {
        var errors = new[] { "Error 1" };
        var result = ApiResponse.InternalServerError("Server error", errors);

        var response = ((ObjectResult)result).Value.Should().BeOfType<ErrorResponse>().Which;
        response.Errors.Should().BeEquivalentTo("Error 1");
    }

    [Fact]
    public void Ok_WithNullData_ReturnsSuccessResponse()
    {
        var result = ApiResponse.Ok<string>(null);

        var response = ((OkObjectResult)result).Value.Should().BeOfType<Response<string>>().Which;
        response.Success.Should().BeTrue();
        response.Data.Should().BeNull();
    }
}
