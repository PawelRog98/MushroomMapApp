using Microsoft.EntityFrameworkCore;
using MushroomMapApp.Domain.Entities;
using System.Reflection;

namespace MushroomMapApp.Domain.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Token> Tokens => Set<Token>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<Reaction> Reactions => Set<Reaction>();
    public DbSet<ReactionType> ReactionTypes => Set<ReactionType>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("pgcrypto");
        modelBuilder.HasPostgresExtension("uuid-ossp");
        modelBuilder.HasPostgresExtension("postgis");

        base.OnModelCreating(modelBuilder);

        var configurator = new EntitiesBuilderConfiguration();
        configurator.Configure(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
