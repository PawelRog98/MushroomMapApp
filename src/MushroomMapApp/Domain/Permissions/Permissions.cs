namespace MushroomMapApp.Domain.Permissions;

public record PermissionDefinition(string Code, string Name, params string[] Implies);
public static class Permissions
{
    public static readonly IReadOnlyList<string> All =
    [
        Locations.View.Code,
        Locations.Edit.Code,
        Locations.Delete.Code,
        AdministratorDashboard.View.Code,
        AdministratorDashboard.PermissionsEdit.Code,
        AdministratorDashboard.UserManagment.Code
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

    public static class AdministratorDashboard
    {
        public static readonly PermissionDefinition View =
            new ("administrator.view",
                "View administrator dashboard");

        public static readonly PermissionDefinition PermissionsEdit =
            new ("administrator.permissions",
                "Edit permissions",
                View.Code);

        public static readonly PermissionDefinition UserManagment =
            new("administrator.users",
                "User management",
                View.Code);
    }
}
