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
            .ReturnsAsync(new Dictionary<Guid, LocationPermissionResult>());

        _handler = new GetLocationQueryHandler(_context, _evaluatorMock.Object, _contextFactoryMock.Object);
    }

    public void Dispose() => _context.Dispose();

    private static Point CreatePoint(double lng, double lat) => new(lng, lat);

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
    public async Task Handle_ReturnsLocations_WithinBoundingBox()
    {
        await SeedLocationAsync("Inside", 19.0, 50.0, 1);

        var query = new GetLocationQuery(new GetLocationRequest(null, -90, -180, 90, 180));

        var (items, permissions) = await _handler.Handle(query, CancellationToken.None);

        items.Should().ContainSingle();
        items.First().Name.Should().Be("Inside");
        items.First().Lat.Should().Be(50.0);
        items.First().Lng.Should().Be(19.0);
    }

    [Fact]
    public async Task Handle_FiltersLocations_BySearchTerm()
    {
        await SeedLocationAsync("Mushroom spot", 19.0, 50.0, 1);
        await SeedLocationAsync("Other place", 19.0, 50.0, 1);

        var query = new GetLocationQuery(new GetLocationRequest("Mushroom", -90, -180, 90, 180));

        var (items, _) = await _handler.Handle(query, CancellationToken.None);

        items.Should().ContainSingle();
        items.First().Name.Should().Be("Mushroom spot");
    }

    [Fact]
    public async Task Handle_ReturnsEmptyList_WhenNoLocationsMatch()
    {
        await SeedLocationAsync("Mushroom spot", 19.0, 50.0, 1);

        var query = new GetLocationQuery(new GetLocationRequest("NonExistent", -90, -180, 90, 180));

        var (items, _) = await _handler.Handle(query, CancellationToken.None);

        items.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ReturnsAuthorInfo()
    {
        var location = await SeedLocationAsync("Test", 19.0, 50.0, 1);

        var query = new GetLocationQuery(new GetLocationRequest(null, -90, -180, 90, 180));

        var (items, _) = await _handler.Handle(query, CancellationToken.None);

        items.Should().ContainSingle();
        items.First().AuthorName.Should().Be("author");
        items.First().AuthorPublicId.Should().Be(location.CreatedBy.PublicId);
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

        var query = new GetLocationQuery(new GetLocationRequest(null, -90, -180, 90, 180));

        var (items, _) = await _handler.Handle(query, CancellationToken.None);

        items.Should().ContainSingle();
        items.First().Images.Should().ContainSingle();
        items.First().Images.First().PublicId.Should().Be(image.PublicId);
        items.First().Images.First().ThumbnailUrl.Should().Be("thumb.jpg");
        items.First().Images.First().ContentType.Should().Be("image/jpeg");
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

        var query = new GetLocationQuery(new GetLocationRequest(null, -90, -180, 90, 180));

        var (_, resultPermissions) = await _handler.Handle(query, CancellationToken.None);

        resultPermissions.Should().ContainKey(location.PublicId);
        resultPermissions[location.PublicId].CanView.Should().BeTrue();
        resultPermissions[location.PublicId].CanEdit.Should().BeTrue();
        resultPermissions[location.PublicId].CanDelete.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_UsesContextFactory_ToCreatePermissionContext()
    {
        await SeedLocationAsync("Test", 19.0, 50.0, 1);

        var query = new GetLocationQuery(new GetLocationRequest(null, -90, -180, 90, 180));

        await _handler.Handle(query, CancellationToken.None);

        _contextFactoryMock.Verify(x => x.Create(It.IsAny<CancellationToken>()), Times.Once);
    }
}
