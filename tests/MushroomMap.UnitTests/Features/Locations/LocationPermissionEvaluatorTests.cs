using FluentAssertions;
using MushroomMapApp.Domain.Entities;
using MushroomMapApp.Domain.Permissions;
using MushroomMapApp.Features.Locations.GetLocations;
using Xunit;

namespace MushroomMap.UnitTests.Features.Locations;

public class LocationPermissionEvaluatorTests
{
    private readonly LocationPermissionEvaluator _evaluator = new();

    private static Location CreateLocation(Guid publicId, long createdById) => new()
    {
        PublicId = publicId,
        Name = "Test",
        Text = "Test",
        CreatedById = createdById
    };

    [Fact]
    public async Task Evaluate_ReturnsCanView_ForAllLocations()
    {
        var locations = new[] { CreateLocation(Guid.NewGuid(), 1) };
        var context = new LocationPermissionContext { UserId = 1, Permissions = new HashSet<string>() };

        var result = await _evaluator.Evaluate(locations, context, CancellationToken.None);

        result.Should().ContainKey(locations[0].PublicId);
        result[locations[0].PublicId].CanView.Should().BeTrue();
    }

    [Fact]
    public async Task Evaluate_ReturnsCanEdit_ForOwner()
    {
        var location = CreateLocation(Guid.NewGuid(), 1);
        var context = new LocationPermissionContext { UserId = 1, Permissions = new HashSet<string>() };

        var result = await _evaluator.Evaluate(new[] { location }, context, CancellationToken.None);

        result[location.PublicId].CanEdit.Should().BeTrue();
    }

    [Fact]
    public async Task Evaluate_ReturnsCanEditFalse_ForNonOwner()
    {
        var location = CreateLocation(Guid.NewGuid(), 1);
        var context = new LocationPermissionContext { UserId = 2, Permissions = new HashSet<string>() };

        var result = await _evaluator.Evaluate(new[] { location }, context, CancellationToken.None);

        result[location.PublicId].CanEdit.Should().BeFalse();
    }

    [Fact]
    public async Task Evaluate_ReturnsCanDelete_ForOwner()
    {
        var location = CreateLocation(Guid.NewGuid(), 1);
        var context = new LocationPermissionContext { UserId = 1, Permissions = new HashSet<string>() };

        var result = await _evaluator.Evaluate(new[] { location }, context, CancellationToken.None);

        result[location.PublicId].CanDelete.Should().BeTrue();
    }

    [Fact]
    public async Task Evaluate_ReturnsCanDelete_ForNonOwnerWithDeletePermission()
    {
        var location = CreateLocation(Guid.NewGuid(), 1);
        var context = new LocationPermissionContext
        {
            UserId = 2,
            Permissions = new HashSet<string> { Permissions.Locations.Delete.Code }
        };

        var result = await _evaluator.Evaluate(new[] { location }, context, CancellationToken.None);

        result[location.PublicId].CanDelete.Should().BeTrue();
    }

    [Fact]
    public async Task Evaluate_ReturnsCanDeleteFalse_ForNonOwnerWithoutDeletePermission()
    {
        var location = CreateLocation(Guid.NewGuid(), 1);
        var context = new LocationPermissionContext { UserId = 2, Permissions = new HashSet<string>() };

        var result = await _evaluator.Evaluate(new[] { location }, context, CancellationToken.None);

        result[location.PublicId].CanDelete.Should().BeFalse();
    }

    [Fact]
    public async Task Evaluate_ReturnsResults_ForMultipleLocations()
    {
        var location1 = CreateLocation(Guid.NewGuid(), 1);
        var location2 = CreateLocation(Guid.NewGuid(), 2);
        var context = new LocationPermissionContext { UserId = 1, Permissions = new HashSet<string>() };

        var result = await _evaluator.Evaluate(new[] { location1, location2 }, context, CancellationToken.None);

        result.Should().HaveCount(2);
        result.Should().ContainKey(location1.PublicId);
        result.Should().ContainKey(location2.PublicId);
    }

    [Fact]
    public async Task Evaluate_ReturnsEmpty_ForNoLocations()
    {
        var context = new LocationPermissionContext { UserId = 1, Permissions = new HashSet<string>() };

        var result = await _evaluator.Evaluate(Array.Empty<Location>(), context, CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Evaluate_CanDeleteTrue_ForOwnerEvenWithoutDeletePermission()
    {
        var location = CreateLocation(Guid.NewGuid(), 1);
        var context = new LocationPermissionContext { UserId = 1, Permissions = new HashSet<string>() };

        var result = await _evaluator.Evaluate(new[] { location }, context, CancellationToken.None);

        result[location.PublicId].CanDelete.Should().BeTrue();
    }
}
