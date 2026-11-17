namespace MushroomMapApp.Features.Locations.GetLocations;

public class MapGrid
{
    public const int MinZoom = 0;
    public const int MaxZoom = 16;
    public const int DefaultZoom = 10;
    public const int ClusterZoomThreshold = 12;
    public const int MaxMarkers = 500;
    public const int MaxClusters = 500;

    public static int ClampZoom(int zoom) =>
        Math.Clamp(zoom, MinZoom, MaxZoom);

    public static double CellSizeDegrees(int zoom) =>
        90/ Math.Pow(2, ClampZoom(zoom));
}
