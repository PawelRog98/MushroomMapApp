namespace MushroomMapApp.Domain.Permissions;

public record PermissionDefinition(string Code, string Name, params string[] Implies);
public static class Permissions
{
    public static readonly IReadOnlyList<string> All =
    [
        Locations.View.Code,
        Locations.Edit.Code,
        Locations.Delete.Code
    ];

    public static class Locations
    {
        public static readonly PermissionDefinition View =
            new("locations.view",
                "View locations");

        public static readonly PermissionDefinition Edit =
            new("locations.edit",
                "Edit locations",
                View.Code);

        public static readonly PermissionDefinition Delete =
            new("locations.delete",
                "Delete locations",
                Edit.Code);
    }
}
