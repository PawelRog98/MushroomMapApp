namespace MushroomMap.IntegrationTests.Helpers;

public static class TestData
{
    public const string Password = "Password-1";
    public const string AdminEmail = "admin1@admin.com";

    public static string UniqueEmail(string prefix = "user")
        => $"{prefix}-{Guid.NewGuid():N}@example.test";

    public static string UniqueNick(string prefix = "nick")
        => $"{prefix}{Guid.NewGuid():N}"[..24];

    public static string UniqueLocationName(string prefix = "Location")
        => $"{prefix} {Guid.NewGuid():N}";
}
