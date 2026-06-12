using System.Text;
using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using MushroomMapApp.Configuration;
using MushroomMapApp.Domain.Data;
using MushroomMapApp.Domain.Entities;
using MushroomMapApp.Domain.Interfaces;
using MushroomMapApp.Domain.Models;
using MushroomMapApp.Features.Common.Behaviors;
using MushroomMapApp.Features.Users.Login;
using MushroomMapApp.Features.Users.Register;
using MushroomMapApp.Infrastructure.Middlewares;
using MushroomMapApp.Infrastructure.Services;
using MushroomMapApp.Shared.Response;
using FluentValidation;
using Hangfire;
using Hangfire.PostgreSql;
using Npgsql;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);
var mushroomMapSpecificOrigins = "_mushroomMapSpecificOrigins";

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

#region CQRS
builder.Services.AddCors(options =>
{
    var urls = builder.Configuration.GetSection("CORS:CorsUrls").Get<List<string>>();

    foreach (var url in urls)
    {
        Console.Error.WriteLine("[Program cqrs]: :"+ url);
    }
    options.AddPolicy(name: mushroomMapSpecificOrigins, policy =>
    {
        if (urls != null && urls.Any())
        {
            policy.WithOrigins(urls.ToArray())
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        }
        else
        {
            policy.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
    });
});


#endregion

#region Connections


var redisConnection = builder.Configuration.GetConnectionString("RedisConnection");
var redis = ConnectionMultiplexer.Connect(redisConnection);

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnection;
    options.InstanceName = "AppCacheData_";
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddHangfire(conf =>
    conf.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UsePostgreSqlStorage(options => 
            options.UseNpgsqlConnection(connectionString), new PostgreSqlStorageOptions
            {
                QueuePollInterval = TimeSpan.FromSeconds(5),
                PrepareSchemaIfNecessary = true,
                SchemaName = "hangfire",
                InvisibilityTimeout = TimeSpan.FromMinutes(5),
                DistributedLockTimeout = TimeSpan.FromMinutes(10),
                UseNativeDatabaseTransactions = true
            }));

builder.Services.AddHangfireServer(options =>
{
    options.WorkerCount = Environment.ProcessorCount * 5;
    options.ServerName = "MushroomMapApp_Server";
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));


#endregion

builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<IAuthService, AuthService>();

#region Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MushroomMapApp API",
        Version = "v1",
        Description = "MushroomMapApp API",
    });
    options.CustomSchemaIds(type => type.FullName?.Replace("+", "."));
});

#endregion


builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

#region JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.JwtIssuer,
            ValidAudience = jwtSettings.JwtIssuer,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.JwtKey))
        };
        options.Events = new JwtBearerEvents
        {
            OnChallenge = async context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";
                var response = new ErrorResponse("Unauthorized access");
                var json = JsonSerializer.Serialize(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                await context.Response.WriteAsync(json);
            },
            OnForbidden = async context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";
                var response = new ErrorResponse("Forbidden access");
                var json = JsonSerializer.Serialize(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                await context.Response.WriteAsync(json);
            }
        };
    });

#endregion
builder.Services.AddScoped<DbSeeder>();

builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
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

app.MapRegisterEndpoint();
app.MapLoginEndpoint();

app.Run();
