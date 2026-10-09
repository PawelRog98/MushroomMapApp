namespace MushroomMap.IntegrationTests.Fixtures;

public static class TestConfiguration
{
    public const string JwtKey = "integration-tests-jwt-signing-key-with-at-least-32-bytes-0123456789abcdef";
    public const string JwtIssuer = "MushroomMapApp.IntegrationTests";

    public static string StorageRoot { get; } =
        Path.Combine(Path.GetTempPath(), $"mushroommap-tests-{Guid.NewGuid():N}");

    public static void Apply(string postgresConnectionString, string redisConnectionString)
    {
        Environment.SetEnvironmentVariable("SQL_CONN", postgresConnectionString);
        Environment.SetEnvironmentVariable("REDIS_CONN", redisConnectionString);
        Environment.SetEnvironmentVariable("FILES_STORAGE", StorageRoot);
        Environment.SetEnvironmentVariable("JWT_KEY", JwtKey);
        Environment.SetEnvironmentVariable("JWT_ISSUER", JwtIssuer);
        Environment.SetEnvironmentVariable("JWT_TIME_EXPIRE_MINUTES", "60");
        Environment.SetEnvironmentVariable("API_URL", "http://localhost");
        Environment.SetEnvironmentVariable("EMAIL_HOST", "smtp.tests.local");
        Environment.SetEnvironmentVariable("EMAIL_PORT", "587");
        Environment.SetEnvironmentVariable("EMAIL", "test@test.test");
        Environment.SetEnvironmentVariable("EMAIL_USERNAME", "tests");
        Environment.SetEnvironmentVariable("EMAIL_PASSWORD", "tests");
        Environment.SetEnvironmentVariable("EMAIL_FROMNAME", "MushroomMapTests");
    }
}
