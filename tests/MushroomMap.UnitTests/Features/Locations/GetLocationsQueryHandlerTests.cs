using FluentAssertions;
using Moq;
using MushroomMap.UnitTests.Common.Database;
using MushroomMapApp.Domain.Data;
using MushroomMapApp.Domain.Entities;
using MushroomMapApp.Domain.Enums;
using MushroomMapApp.Domain.Interfaces;
using MushroomMapApp.Features.Locations.GetLocations;
using NetTopologySuite.Geometries;
using Xunit;
using LocationEntity = MushroomMapApp.Domain.Entities.Location;

namespace MushroomMap.UnitTests.Features.Locations;

public class GetLocationsQueryHandlerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<IResourcePermissionEvaluator<LocationEntity, LocationPermissionContext, LocationPermissionResult>> _evaluatorMock;
    private readonly Mock<IPermissionsContextFactory<LocationPermissionContext>> _contextFactoryMock;
    private readonly GetLocationQueryHandler _handler;

    public GetLocationsQueryHandlerTests()
    {
        _context = TestDbContextFactory.Create();
        _evaluatorMock = new Mock<IResourcePermissionEvaluator<LocationEntity, LocationPermissionContext, LocationPermissionResult>>();
        _contextFactoryMock = new Mock<IPermissionsContextFactory<LocationPermissionContext>>();

        _contextFactoryMock
            .Setup(x => x.Create(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LocationPermissionContext { UserId = 1, Permissions = new HashSet<string>() });

        _evaluatorMock
            .Setup(x => x.Evaluate(It.IsAny<IEnumerable<LocationEntity>>(), It.IsAny<LocationPermissionContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IEnumerable<LocationEntity> locations, LocationPermissionContext _, CancellationToken _) =>
                locations.ToDictionary(x => x.PublicId, _ => new LocationPermissionResult { CanView = true }));

        _handler = new GetLocationQueryHandler(_context, _evaluatorMock.Object, _contextFactoryMock.Object);
    }

    public void Dispose() => _context.Dispose();

    private static Point CreatePoint(double lng, double lat) => new(lng, lat);

    private static GetLocationRequest WorldRequest(string? search = null, int zoom = MapGrid.MaxZoom)
        => new(search, -90, -180, 90, 180, zoom);

    private async Task<LocationEntity> SeedLocationAsync(string name, double lng, double lat, long createdById)
    {
        var author = _context.Users.FirstOrDefault(u => u.Id == createdById);
        if (author is null)
        {
            author = new User { PublicId = Guid.NewGuid(), PublicNick = "author", FirstName = "Author", LastName = "Test", Email = $"author{createdById}@test.com", PasswordHash = "hash" };
            _context.Users.Add(author);
            await _context.SaveChangesAsync();
        }

        var location = new LocationEntity
        {
            PublicId = Guid.NewGuid(),
            Name = name,
            Text = "Test location",
            Coordinates = CreatePoint(lng, lat),
            CreatedById = author.Id,
            CreatedBy = author
        };
        _context.Locations.Add(location);
        await _context.SaveChangesAsync();
        return location;
    }

    [Fact]
    public async Task Handle_InMarkersMode_ReturnsLocationsWithinBoundingBox()
    {
        await SeedLocationAsync("Inside", 19.0, 50.0, 1);

        var query = new GetLocationQuery(WorldRequest());

        var response = await _handler.Handle(query, CancellationToken.None);

        response.Mode.Should().Be("markers");
        response.TotalCount.Should().Be(1);
        response.Truncated.Should().BeFalse();
        response.Markers.Should().ContainSingle();
        response.Markers.First().Data.Name.Should().Be("Inside");
        response.Markers.First().Data.Lat.Should().Be(50.0);
        response.Markers.First().Data.Lng.Should().Be(19.0);
    }

    [Fact]
    public async Task Handle_FiltersLocations_BySearchTerm()
    {
        await SeedLocationAsync("Mushroom spot", 19.0, 50.0, 1);
        await SeedLocationAsync("Other place", 19.0, 50.0, 1);

        var query = new GetLocationQuery(WorldRequest("Mushroom"));

        var response = await _handler.Handle(query, CancellationToken.None);

        response.Markers.Should().ContainSingle();
        response.Markers.First().Data.Name.Should().Be("Mushroom spot");
        response.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ReturnsEmptyMarkers_WhenNoLocationsMatch()
    {
        await SeedLocationAsync("Mushroom spot", 19.0, 50.0, 1);

        var query = new GetLocationQuery(WorldRequest("NonExistent"));

        var response = await _handler.Handle(query, CancellationToken.None);

        response.Mode.Should().Be("markers");
        response.Markers.Should().BeEmpty();
        response.TotalCount.Should().Be(0);
        response.Truncated.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ReturnsAuthorInfo()
    {
        var location = await SeedLocationAsync("Test", 19.0, 50.0, 1);

        var query = new GetLocationQuery(WorldRequest());

        var response = await _handler.Handle(query, CancellationToken.None);

        response.Markers.Should().ContainSingle();
        response.Markers.First().Data.AuthorName.Should().Be("author");
        response.Markers.First().Data.AuthorPublicId.Should().Be(location.CreatedBy.PublicId);
    }

    [Fact]
    public async Task Handle_ReturnsImages_ForLocation()
    {
        var location = await SeedLocationAsync("Test", 19.0, 50.0, 1);

        var image = new FileResource
        {
            PublicId = Guid.NewGuid(),
            FileName = "image.jpg",
            ContentType = "image/jpeg",
            Type = FileType.Image.ToString(),
            LocationId = location.Id
        };

        var thumbnail = new FileResource
        {
            PublicId = Guid.NewGuid(),
            FileName = "thumb.jpg",
            ContentType = "image/jpeg",
            Type = FileType.Thumbnail.ToString(),
            ParentFileResource = image
        };

        image.Variant.Add(thumbnail);

        _context.FileResources.Add(image);
        await _context.SaveChangesAsync();

        var query = new GetLocationQuery(WorldRequest());

        var response = await _handler.Handle(query, CancellationToken.None);

        var item = response.Markers.Should().ContainSingle().Subject.Data;
        item.Images.Should().ContainSingle();
        item.Images.First().PublicId.Should().Be(image.PublicId);
        item.Images.First().ThumbnailUrl.Should().Be("thumb.jpg");
        item.Images.First().ContentType.Should().Be("image/jpeg");
    }

    [Fact]
    public async Task Handle_ReturnsPermissions_ForMarkers()
    {
        var location = await SeedLocationAsync("Test", 19.0, 50.0, 1);

        var permissions = new Dictionary<Guid, LocationPermissionResult>
        {
            [location.PublicId] = new() { CanView = true, CanEdit = true, CanDelete = false }
        };

        _evaluatorMock
            .Setup(x => x.Evaluate(It.IsAny<IEnumerable<LocationEntity>>(), It.IsAny<LocationPermissionContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(permissions);

        var query = new GetLocationQuery(WorldRequest());

        var response = await _handler.Handle(query, CancellationToken.None);

        var meta = response.Markers.Should().ContainSingle().Subject.Meta as LocationPermissionResult;
        meta.Should().NotBeNull();

        var permission = meta!;
        permission.CanView.Should().BeTrue();
        permission.CanEdit.Should().BeTrue();
        permission.CanDelete.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ReturnsClusters_WhenZoomIsBelowThreshold()
    {
        await SeedLocationAsync("Nearby A", 19.0, 50.0, 1);
        await SeedLocationAsync("Nearby B", 19.001, 50.001, 1);
        await SeedLocationAsync("Far away", 20.0, 51.0, 1);

        var zoom = MapGrid.DefaultZoom;
        var query = new GetLocationQuery(WorldRequest(zoom: zoom));

        var response = await _handler.Handle(query, CancellationToken.None);

        response.Mode.Should().Be("clusters");
        response.TotalCount.Should().Be(3);
        response.Markers.Should().BeEmpty();
        response.Clusters.Should().HaveCount(2);

        var cluster = response.Clusters.Single(x => x.Count == 2);
        cluster.Zoom.Should().Be(zoom);
        cluster.South.Should().Be(50.0);
        cluster.North.Should().Be(50.001);
        cluster.West.Should().Be(19.0);
        cluster.East.Should().Be(19.001);
        cluster.Lat.Should().BeApproximately(50.0005, 0.0001);
        cluster.Lng.Should().BeApproximately(19.0005, 0.0001);
        cluster.CellX.Should().Be((long)Math.Floor(19.0 / MapGrid.CellSizeDegrees(zoom)));
        cluster.CellY.Should().Be((long)Math.Floor(50.0 / MapGrid.CellSizeDegrees(zoom)));
    }

    [Fact]
    public async Task Handle_ReturnsMarkers_WhenZoomIsAtThreshold()
    {
        await SeedLocationAsync("Boundary", 19.0, 50.0, 1);

        var query = new GetLocationQuery(WorldRequest(zoom: MapGrid.ClusterZoomThreshold));

        var response = await _handler.Handle(query, CancellationToken.None);

        response.Mode.Should().Be("markers");
        response.Markers.Should().ContainSingle();
    }

    [Fact]
    public async Task Handle_DoesNotEvaluatePermissions_InClustersMode()
    {
        await SeedLocationAsync("Test", 19.0, 50.0, 1);

        var query = new GetLocationQuery(WorldRequest(zoom: MapGrid.DefaultZoom));

        await _handler.Handle(query, CancellationToken.None);

        _contextFactoryMock.Verify(x => x.Create(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_UsesContextFactory_ToCreatePermissionContext()
    {
        await SeedLocationAsync("Test", 19.0, 50.0, 1);

        var query = new GetLocationQuery(WorldRequest());

        await _handler.Handle(query, CancellationToken.None);

        _contextFactoryMock.Verify(x => x.Create(It.IsAny<CancellationToken>()), Times.Once);
    }
}
