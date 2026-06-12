using DotNetEnv;

namespace MushroomMapApp.Configuration
{
    public static class ConfigurationExtensions
    {
        public static IConfigurationBuilder AddMainConfiguration(this IConfigurationBuilder builder)
        {
            var launchProfile = Environment.GetEnvironmentVariable("DEVELOPMENT_PROFILE");

            if (!string.IsNullOrWhiteSpace(launchProfile) && launchProfile == "Linux-local")
            {
                var rootEnv = Path.GetFullPath(
                    Path.Combine(
                        AppContext.BaseDirectory,
                        "..", "..", "..", "..", "..",
                        ".env"));
                
                Env.Load(rootEnv);
            }
            else
            {
                Env.Load();
            }

            var settingDictionary = new Dictionary<string, string>
            {
                { "ConnectionStrings:DefaultConnection", Environment.GetEnvironmentVariable("SQL_CONN") ?? "" },
                { "ConnectionStrings:RedisConnection", Environment.GetEnvironmentVariable("REDIS_CONN") ?? "" },
                { "ApiUrls:MainUrl", Environment.GetEnvironmentVariable("API_URL") ?? "" },
                { "JWTAuth:JwtKey", Environment.GetEnvironmentVariable("JWT_KEY") ?? "" },
                { "JWTAuth:JwtIssuer", Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "" },
                { "JWTAuth:JwtExpireMinutes", Environment.GetEnvironmentVariable("JWT_TIME_EXPIRE_MINUTES") ?? "0" },
                { "FileStorage:RootPath", Environment.GetEnvironmentVariable("FILES_STORAGE") ?? "" },
                { "Hangfire:Dashboard:Username", Environment.GetEnvironmentVariable("HANGFIRE_USERNAME") ?? "" },
                { "Hangfire:Dashboard:Password", Environment.GetEnvironmentVariable("HANGFIRE_PASSWORD") ?? "" },
                { "EmailConfiguration:Host",  Environment.GetEnvironmentVariable("EMAIL_HOST") ?? "" },
                { "EmailConfiguration:Port",  Environment.GetEnvironmentVariable("EMAIL_PORT") ?? "" },
                { "EmailConfiguration:FromEmail",  Environment.GetEnvironmentVariable("EMAIL") ?? "" },
                { "EmailConfiguration:Username",  Environment.GetEnvironmentVariable("EMAIL_USERNAME") ?? "" },
                { "EmailConfiguration:Password",  Environment.GetEnvironmentVariable("EMAIL_PASSWORD") ?? "" },
                { "EmailConfiguration:FromName",  Environment.GetEnvironmentVariable("EMAIL_FROMNAME") ?? "" },
            };

            Console.Error.WriteLine("--- [DIAGNOSTIC] STARTING CONFIGURATION LOADING ---");
            foreach (var setting in settingDictionary)
            {
                if (setting.Key == "JWTAuth:JwtKey")
                {
                    Console.Error.WriteLine($"[Config] {setting.Key} Length: {setting.Value.Length}");
                }
                else
                {
                    Console.Error.WriteLine($"[Config] {setting.Key}: {setting.Value}");
                }
            }
            Console.Error.WriteLine("--- [DIAGNOSTIC] FINISHED CONFIGURATION LOADING ---");

            var corsUrls = Environment.GetEnvironmentVariable("CORS_URLS");
            if (!string.IsNullOrEmpty(corsUrls))
            {
                var urls = corsUrls.Split(',', StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < urls.Length; i++)
                {
                    settingDictionary[$"CORS:CorsUrls:{i}"] = urls[i].Trim();
                }
            }

            return builder.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddInMemoryCollection(settingDictionary!)
                .AddEnvironmentVariables();
        }
    }
}
