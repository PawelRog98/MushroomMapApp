using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MushroomMapApp.Domain.Entities;
using MushroomMapApp.Domain.Interfaces;

namespace MushroomMapApp.Domain.Data;

public class DbSeeder
{
    private readonly AppDbContext _context;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IPermissionsSynchronizer _permissionsSynchronizer;

    public DbSeeder(
        AppDbContext context,
        IPasswordHasher<User> passwordHasher,
        IPermissionsSynchronizer permissionsSynchronizer)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _permissionsSynchronizer = permissionsSynchronizer;
    }


    public async Task SeedAsync()
    {
        await DbHealtCheck();

        if (!await _context.Roles.AnyAsync())
        {
            await _context.Roles.AddRangeAsync(GetRoles());
            await _context.SaveChangesAsync();
        }

        if (!await _context.Users.AnyAsync())
        {
            var adminRole = await _context.Roles
                .FirstOrDefaultAsync(x => x.Name == "Administrator");

            if (adminRole == null)
                throw new Exception("Role Admin not found in db");

            await _context.Users.AddRangeAsync(GetUsers(adminRole.Id));
            await _context.SaveChangesAsync();
        }

        if (!await _context.ReactionTypes.AnyAsync())
        {
            await _context.ReactionTypes.AddRangeAsync(GetReactionTypes());
            await _context.SaveChangesAsync();
        }

        await _permissionsSynchronizer.Synchronize(CancellationToken.None);

        if (!await _context.UserRoles.AnyAsync())
        {
            var adminUser = await _context.Users
                .FirstOrDefaultAsync(x => x.Email == "admin1@admin.com");
            var adminRole = await _context.Roles
                .FirstOrDefaultAsync(x => x.Name == "Administrator");

            if (adminUser != null && adminRole != null)
            {
                _context.UserRoles.Add(new UserRole
                {
                    UserId = adminUser.Id,
                    RoleId = adminRole.Id
                });
                await _context.SaveChangesAsync();
            }
        }

        if (!await _context.RolePermissions.AnyAsync())
        {
            var adminRole = await _context.Roles
                .FirstOrDefaultAsync(x => x.Name == "Administrator");
            var allPermissions = await _context.Permissions
                .Where(x => x.IsActive)
                .ToListAsync();

            if (adminRole != null && allPermissions.Count != 0)
            {
                _context.RolePermissions.AddRange(
                    allPermissions.Select(p => new RolePermission
                    {
                        RoleId = adminRole.Id,
                        PermissionId = p.Id
                    }));
                await _context.SaveChangesAsync();
            }
        }
    }

    private IEnumerable<Role> GetRoles()
    {
        return new List<Role>()
        {
            new Role
            {
                Name = "User"
            },
            new Role
            {
                Name = "Administrator"
            }
        };
    }

    private IEnumerable<User> GetUsers(long adminRoleId)
    {
        var users = new List<User>()
        {
            new User
            {
                Email = "admin1@admin.com",
                PublicNick = "Admin",
                FirstName = "Name",
                LastName = "LastName",
                PasswordHash = "Password-1",
                DateOfBirth = DateTime.UtcNow.AddYears(-20),
                IsEmailConfirmed = true,
                AccountInfo = "Admin",
                RoleId = adminRoleId
            }
        };
        foreach (var user in users)
        {
            var password = _passwordHasher.HashPassword(user, user.PasswordHash);
            user.PasswordHash = password;
        }

        return users;
    }

    private IEnumerable<ReactionType> GetReactionTypes()
    {
        var reactionTypes = new List<ReactionType>()
        {
            new ReactionType
            {
                Key = "like",
                Name = "Like",
                Icon = "👍",
                CreatedAtUtc = DateTime.UtcNow
            },
            new ReactionType
            {
                Key = "dislike",
                Name = "Dislike",
                Icon = "👎",
                CreatedAtUtc = DateTime.UtcNow
            },
            new ReactionType
            {
                Key = "love",
                Name = "Love",
                Icon = "❤️",
                CreatedAtUtc = DateTime.UtcNow
            },
            new ReactionType
            {
                Key = "mushroom",
                Name = "Mushroom",
                Icon = "🍄",
                CreatedAtUtc = DateTime.UtcNow
            }
        };

        return reactionTypes;
    }

    private async Task DbHealtCheck()
    {
        int maxRetries = 12;
        int delaySeconds = 15;
        bool isConnected = false;

        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                await _context.Database.OpenConnectionAsync();
                await _context.Database.CloseConnectionAsync();

                isConnected = true;
                Console.WriteLine($"Connected to DB in: {stopwatch.Elapsed}");
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SQL Failed in {i + 1} attempt");
                await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
            }


        }
        stopwatch.Stop();

        if (!isConnected)
        {
            throw new Exception("Database is not available");
        }
    }
}
