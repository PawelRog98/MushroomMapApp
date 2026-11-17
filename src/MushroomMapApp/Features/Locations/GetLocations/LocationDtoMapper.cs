using MushroomMapApp.Domain.Entities;
using MushroomMapApp.Domain.Enums;

namespace MushroomMapApp.Features.Locations.GetLocations;

public static class LocationDtoMapper
{
    public static LocationListItemDto ToListItemDto(Location x) => new()
    {
        PublicId = x.PublicId,
        AuthorName = x.CreatedBy.PublicNick,
        AuthorPublicId = x.CreatedBy.PublicId,
        Name = x.Name,
        Text = x.Text,
        Lat = x.Coordinates.Y,
        Lng = x.Coordinates.X,
        Images = x.FileResources
            .Where(y => y.Type == FileType.Image.ToString())
            .Select(y => new ImageDto
            {
                PublicId = y.PublicId,
                ThumbnailUrl = y.Variant
                    .Where(z => z.Type == FileType.Thumbnail.ToString())
                    .Select(z => z.FileName)
                    .FirstOrDefault(),
                ContentType = y.ContentType
            })
            .ToList()
    };
}
