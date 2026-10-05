using System.Net;
using FluentAssertions;
using MushroomMap.IntegrationTests.Fixtures;
using MushroomMap.IntegrationTests.Helpers;
using MushroomMapApp.Domain.Permissions;
using MushroomMapApp.Features.Users.GetAllUsers;
using Xunit;

namespace MushroomMap.IntegrationTests.Features.Users;

public class GetAllUsersTests : IntegrationTestBase
{
    public GetAllUsersTests(IntegrationTestFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task GetUsers_AsAdministrator_ReturnsUsersWithPagingMetadata()
    {
        var client = await CreateAdminClientAsync();

        var response = await client.GetAsync("/api/users/get-all?page=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.ReadApiPayloadAsync();
        payload.GetProperty("success").GetBoolean().Should().BeTrue();

        var items = payload.GetProperty("data").ToObject<List<UserListItemDto>>()!;
        items.Should().Contain(u => u.Email == TestData.AdminEmail);
        items.Single(u => u.Email == TestData.AdminEmail).RoleName.Should().Be("Administrator");
        items.Single(u => u.Email == TestData.AdminEmail).ActivePermissions
            .Should().BeEquivalentTo(Permissions.All);

        var metadata = payload.GetProperty("metaData");
        metadata.GetProperty("currentPage").GetInt32().Should().Be(1);
        metadata.GetProperty("pageSize").GetInt32().Should().Be(10);
        metadata.GetProperty("totalCount").GetInt32().Should().BeGreaterThanOrEqualTo(items.Count);
        metadata.GetProperty("totalPages").GetInt32().Should().BeGreaterThanOrEqualTo(1);
        metadata.GetProperty("hasPrevious").GetBoolean().Should().BeFalse();
        metadata.GetProperty("sortBy").GetString().Should().Be("nick");
        metadata.GetProperty("sortDir").GetString().Should().Be("asc");
    }

    [Fact]
    public async Task GetUsers_WithPageSize_ReturnsAtMostRequestedAmount()
    {
        var client = await CreateAdminClientAsync();

        var response = await client.GetAsync("/api/users/get-all?page=1&pageSize=1");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.ReadApiPayloadAsync();
        var items = payload.GetProperty("data").ToObject<List<UserListItemDto>>()!;

        items.Should().HaveCount(1);
        payload.GetProperty("metaData").GetProperty("pageSize").GetInt32().Should().Be(1);
        payload.GetProperty("metaData").GetProperty("totalCount").GetInt32()
            .Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task GetUsers_WithDescendingEmailSort_ReturnsUsersOrderedByEmail()
    {
        var client = await CreateAdminClientAsync();
        await CreateUserAsync();
        await CreateUserAsync();

        var response = await client.GetAsync("/api/users/get-all?page=1&pageSize=100&sortBy=email&sortDir=desc");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.ReadApiPayloadAsync();
        var items = payload.GetProperty("data").ToObject<List<UserListItemDto>>()!;

        items.Should().HaveCountGreaterThan(2);
        items.Select(u => u.Email).Should().BeInDescendingOrder();

        var metadata = payload.GetProperty("metaData");
        metadata.GetProperty("sortBy").GetString().Should().Be("email");
        metadata.GetProperty("sortDir").GetString().Should().Be("desc");
    }

    [Fact]
    public async Task GetUsers_WithUnknownSortBy_ReturnsValidationErrors()
    {
        var client = await CreateAdminClientAsync();

        var response = await client.GetAsync("/api/users/get-all?sortBy=unknown");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.ReadApiAsync<object>();
        content.Message.Should().Be("Validation failed");
        content.Errors.Should().Contain("sortBy must be one of: nick, firstname, lastname, email");
    }

    [Fact]
    public async Task GetUsers_WithInvalidPageSize_ReturnsValidationErrors()
    {
        var client = await CreateAdminClientAsync();

        var response = await client.GetAsync("/api/users/get-all?pageSize=1000");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.ReadApiAsync<object>();
        content.Message.Should().Be("Validation failed");
        content.Errors.Should().Contain("'Page Size' must be between 1 and 100. You entered 1000.");
    }

    [Fact]
    public async Task GetUsers_AsRegularUser_ReturnsForbidden()
    {
        var user = await CreateUserAsync();

        var response = await user.Client.GetAsync("/api/users/get-all");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetUsers_WithoutToken_ReturnsUnauthorized()
    {
        var client = CreateClient();

        var response = await client.GetAsync("/api/users/get-all");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
