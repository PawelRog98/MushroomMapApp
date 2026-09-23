using MushroomMapApp.Configuration;
using MushroomMapApp.Domain.Data;
using MushroomMapApp.Domain.Models;
using MushroomMapApp.Features.Common;
using MushroomMapApp.Features.Locations;
using MushroomMapApp.Features.Reactions;
using MushroomMapApp.Features.Users;
using MushroomMapApp.Infrastructure.Jobs;
using MushroomMapApp.Infrastructure.Middlewares;
using MushroomMapApp.Infrastructure.Services;
using MushroomMapApp.Infrastructure.Jobs;
using Hangfire;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using MushroomMapApp.Features.Files;
using MushroomMapApp.Infrastructure.Services.FileStorage;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddMainConfiguration();

var jwtSettings = new JwtSettings();
builder.Configuration.GetSection("JWTAuth").Bind(jwtSettings);

if (string.IsNullOrEmpty(jwtSettings.JwtKey))
{
    var msg = "JWT Key is missing from configuration. Build 20260609-1. " +
              $"Environment JWT_KEY length: {Environment.GetEnvironmentVariable("JWT_KEY")?.Length ?? 0}";
    Console.Error.WriteLine($"[CRITICAL] {msg}");
    throw new InvalidOperationException(msg);
}

builder.Services.AddSingleton(jwtSettings);

var redisConnection = builder.Configuration.GetConnectionString("RedisConnection")!;
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddHttpContextAccessor();
var mushroomMapSpecificOrigins = builder.Services.AddCorsPolicy(builder.Configuration);
builder.Services.AddInfrastructureServices(redisConnection, builder.Configuration);
builder.Services.AddPersistence(connectionString);
builder.Services.AddBackgroundJobs(connectionString);
builder.Services.AddJobs();
builder.Services.AddJwtAuthentication(jwtSettings);
builder.Services.AddCommonFeatures();
builder.Services.AddLocationsFeature();
builder.Services.AddSwaggerDocs();

var app = builder.Build();

app.UseCors(mushroomMapSpecificOrigins);

app.UseGlobalExceptionHandling();
app.UseRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "MushroomMapApp API v1");
    });
}
else
{
    app.UseHttpsRedirection();
}

if (!app.Environment.IsEnvironment("IntegrationTests"))
{
    using var scope = app.Services.CreateScope();
    var seeder = scope.ServiceProvider.GetRequiredService<DbSeeder>();
    await seeder.SeedAsync();
}

var fileStorageOptions = app.Services.GetRequiredService<IOptions<FileStorageOptions>>();
var thumbnailsPath = Path.Combine(app.Environment.ContentRootPath, fileStorageOptions.Value.RootPath, "thumbnails");
Directory.CreateDirectory(thumbnailsPath);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(thumbnailsPath),
    RequestPath = "/thumbnails"
});

app.UseCors(mushroomMapSpecificOrigins);

app.UseAuthentication();
app.UseAuthorization();

using (var jobScope = app.Services.CreateScope())
    jobScope.ServiceProvider.GetRequiredService<JobRegistrar>().Register();

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    DashboardTitle = "MushroomMapApp Job Dashboard",
    AppPath = "/swagger",
    DefaultRecordsPerPage = 20,
    StatsPollingInterval = 5000,
    DarkModeEnabled = true
});

app.MapUsersEndpoints();
app.MapLocationsEndpoints();
app.MapReactionsEndpoints();
app.MapFileEndpoints();

app.Run();
