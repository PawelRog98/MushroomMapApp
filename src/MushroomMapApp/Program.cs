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
using Hangfire;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddMainConfiguration();

var jwtSettings = new JwtSettings();
builder.Configuration.GetSection("JWTAuth").Bind(jwtSettings);

Console.Error.WriteLine("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@");
Console.Error.WriteLine($"[DIAGNOSTIC BUILD 20260609-1] JWT Issuer: {jwtSettings.JwtIssuer}");
Console.Error.WriteLine($"[DIAGNOSTIC BUILD 20260609-1] JWT Key Length: {jwtSettings.JwtKey?.Length ?? 0}");
Console.Error.WriteLine($"[DIAGNOSTIC BUILD 20260609-1] ENV JWT_KEY Length: {Environment.GetEnvironmentVariable("JWT_KEY")?.Length ?? 0}");
Console.Error.WriteLine("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@");

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

var mushroomMapSpecificOrigins = builder.Services.AddCorsPolicy(builder.Configuration);
builder.Services.AddInfrastructureServices(redisConnection);
builder.Services.AddPersistence(connectionString);
builder.Services.AddBackgroundJobs(connectionString);
builder.Services.AddJwtAuthentication(jwtSettings);
builder.Services.AddCommonFeatures();
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

app.UseCors(mushroomMapSpecificOrigins);

app.UseAuthentication();
app.UseAuthorization();

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

app.Run();

