using FluentAssertions;
using Moq;
using MushroomMap.UnitTests.Common.Database;
using MushroomMapApp.Domain.Data;
using MushroomMapApp.Domain.Entities;
using MushroomMapApp.Domain.Interfaces;
using MushroomMapApp.Features.Locations.GetClusterLocations;
using MushroomMapApp.Features.Locations.GetLocations;
using NetTopologySuite.Geometries;
using Xunit;
using LocationEntity = MushroomMapApp.Domain.Entities.Location;

namespace MushroomMap.UnitTests.Features.Locations;

public class GetClusterLocationsQueryHandlerTests : IDisposable
{
    private const int Zoom = MapGrid.DefaultZoom;

    private readonly AppDbContext _context;
    private readonly Mock<IResourcePermissionEvaluator<LocationEntity, LocationPermissionContext, LocationPermissionResult>> _evaluatorMock;
    private readonly Mock<IPermissionsContextFactory<LocationPermissionContext>> _contextFactoryMock;
    private readonly GetClusterLocationsQueryHandler _handler;

    public GetClusterLocationsQueryHandlerTests()
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

        _handler = new GetClusterLocationsQueryHandler(_context, _evaluatorMock.Object, _contextFactoryMock.Object);
    }

    public void Dispose() => _context.Dispose();

    private static long CellIndex(double coordinate, int zoom)
        => (long)Math.Floor(coordinate / MapGrid.CellSizeDegrees(zoom));

    private static GetClusterLocationsRequest Request(
        double lng,
        double lat,
        string? search = null,
        int page = 1,
        int pageSize = 25,
        int zoom = Zoom)
        => new(CellIndex(lng, zoom), CellIndex(lat, zoom), zoom, search, page, pageSize);

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
            Coordinates = new Point(lng, lat),
            CreatedById = author.Id,
            CreatedBy = author
        };
        _context.Locations.Add(location);
        await _context.SaveChangesAsync();
        return location;
    }

    [Fact]
    public async Task Handle_ReturnsOnlyLocationsInRequestedCell()
    {
        var inside = await SeedLocationAsync("Inside", 19.0, 50.0, 1);
        await SeedLocationAsync("Far away", 20.0, 51.0, 1);

        var query = new GetClusterLocationsQuery(Request(19.0, 50.0));

        var response = await _handler.Handle(query, CancellationToken.None);

        response.TotalCount.Should().Be(1);
        response.Items.Should().ContainSingle();
        response.Items.First().PublicId.Should().Be(inside.PublicId);
        response.Items.First().Lat.Should().Be(50.0);
        response.Items.First().Lng.Should().Be(19.0);
        response.Permissions.Should().ContainKey(inside.PublicId);
    }

    [Fact]
    public async Task Handle_FiltersLocations_BySearchTerm()
    {
        await SeedLocationAsync("Mushroom spot", 19.0, 50.0, 1);
        await SeedLocationAsync("Other place", 19.0, 50.0, 1);

        var query = new GetClusterLocationsQuery(Request(19.0, 50.0, search: "Mushroom"));

        var response = await _handler.Handle(query, CancellationToken.None);

        response.TotalCount.Should().Be(1);
        response.Items.Should().ContainSingle();
        response.Items.First().Name.Should().Be("Mushroom spot");
    }

    [Fact]
    public async Task Handle_PaginatesLocations_AndReportsPageSize()
    {
        await SeedLocationAsync("One", 19.0, 50.0, 1);
        await SeedLocationAsync("Two", 19.001, 50.001, 1);
        await SeedLocationAsync("Three", 19.002, 50.002, 1);

        var query = new GetClusterLocationsQuery(Request(19.0, 50.0, page: 2, pageSize: 1));

        var response = await _handler.Handle(query, CancellationToken.None);

        response.TotalCount.Should().Be(3);
        response.Page.Should().Be(2);
        response.PageSize.Should().Be(1);
        response.Items.Should().ContainSingle();
    }

    [Fact]
    public async Task Handle_ClampsPage_ToLastAvailablePage()
    {
        await SeedLocationAsync("One", 19.0, 50.0, 1);
        await SeedLocationAsync("Two", 19.001, 50.001, 1);

        var query = new GetClusterLocationsQuery(Request(19.0, 50.0, page: 99, pageSize: 1));

        var response = await _handler.Handle(query, CancellationToken.None);

        response.TotalCount.Should().Be(2);
        response.Page.Should().Be(2);
        response.Items.Should().ContainSingle();
    }

    [Fact]
    public async Task Handle_ReturnsEmptyResult_WhenCellHasNoLocations()
    {
        await SeedLocationAsync("Far away", 20.0, 51.0, 1);

        var query = new GetClusterLocationsQuery(Request(19.0, 50.0));

        var response = await _handler.Handle(query, CancellationToken.None);

        response.TotalCount.Should().Be(0);
        response.Items.Should().BeEmpty();
        response.Page.Should().Be(1);
        response.Permissions.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ReturnsPermissions_ForLocations()
    {
        var location = await SeedLocationAsync("Test", 19.0, 50.0, 1);

        var permissions = new Dictionary<Guid, LocationPermissionResult>
        {
            [location.PublicId] = new() { CanView = true, CanEdit = true, CanDelete = false }
        };

        _evaluatorMock
            .Setup(x => x.Evaluate(It.IsAny<IEnumerable<LocationEntity>>(), It.IsAny<LocationPermissionContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(permissions);

        var query = new GetClusterLocationsQuery(Request(19.0, 50.0));

        var response = await _handler.Handle(query, CancellationToken.None);

        response.Permissions.Should().ContainKey(location.PublicId);
        response.Permissions[location.PublicId].CanView.Should().BeTrue();
        response.Permissions[location.PublicId].CanEdit.Should().BeTrue();
        response.Permissions[location.PublicId].CanDelete.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_UsesContextFactory_ToCreatePermissionContext()
    {
        await SeedLocationAsync("Test", 19.0, 50.0, 1);

        var query = new GetClusterLocationsQuery(Request(19.0, 50.0));

        await _handler.Handle(query, CancellationToken.None);

        _contextFactoryMock.Verify(x => x.Create(It.IsAny<CancellationToken>()), Times.Once);
    }
}
