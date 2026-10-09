using FluentAssertions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MushroomMap.UnitTests.Common.Database;
using MushroomMapApp.Domain.Data;
using MushroomMapApp.Domain.Entities;
using MushroomMapApp.Features.Common.Pagination;
using MushroomMapApp.Features.Users.GetAllUsers;
using Xunit;

namespace MushroomMap.UnitTests.Features.Users;

public class UserSortMapTests : IDisposable
{
    private readonly AppDbContext _context;

    public UserSortMapTests()
    {
        _context = TestDbContextFactory.Create();
    }

    public void Dispose() => _context.Dispose();

    private async Task SeedUsersAsync()
    {
        _context.Users.AddRange(
            new User { PublicId = Guid.NewGuid(), PublicNick = "charlie", FirstName = "Charlie", LastName = "Brown", Email = "charlie@test.com", PasswordHash = "hash" },
            new User { PublicId = Guid.NewGuid(), PublicNick = "alice", FirstName = "Alice", LastName = "Smith", Email = "alice@test.com", PasswordHash = "hash" },
            new User { PublicId = Guid.NewGuid(), PublicNick = "bob", FirstName = "Bob", LastName = "Jones", Email = "bob@test.com", PasswordHash = "hash" });
        await _context.SaveChangesAsync();
    }

    [Fact]
    public async Task SortUsers_SortsByNickAscending_ByDefault()
    {
        await SeedUsersAsync();

        var result = _context.Users.AsNoTracking().SortUsers(new SortRequest()).ToList();

        result.Select(x => x.PublicNick).Should().ContainInOrder("alice", "bob", "charlie");
    }

    [Fact]
    public async Task SortUsers_SortsByNickDescending()
    {
        await SeedUsersAsync();

        var result = _context.Users.AsNoTracking().SortUsers(new SortRequest("nick", "desc")).ToList();

        result.Select(x => x.PublicNick).Should().ContainInOrder("charlie", "bob", "alice");
    }

    [Fact]
    public async Task SortUsers_SortsByFirstName()
    {
        await SeedUsersAsync();

        var result = _context.Users.AsNoTracking().SortUsers(new SortRequest("firstname", "asc")).ToList();

        result.Select(x => x.FirstName).Should().ContainInOrder("Alice", "Bob", "Charlie");
    }

    [Fact]
    public async Task SortUsers_SortsByLastName()
    {
        await SeedUsersAsync();

        var result = _context.Users.AsNoTracking().SortUsers(new SortRequest("lastname", "asc")).ToList();

        result.Select(x => x.LastName).Should().ContainInOrder("Brown", "Jones", "Smith");
    }

    [Fact]
    public async Task SortUsers_SortsByEmail()
    {
        await SeedUsersAsync();

        var result = _context.Users.AsNoTracking().SortUsers(new SortRequest("email", "asc")).ToList();

        result.Select(x => x.Email).Should().ContainInOrder("alice@test.com", "bob@test.com", "charlie@test.com");
    }

    [Fact]
    public async Task SortUsers_SortsByEmailDescending()
    {
        await SeedUsersAsync();

        var result = _context.Users.AsNoTracking().SortUsers(new SortRequest("email", "desc")).ToList();

        result.Select(x => x.Email).Should().ContainInOrder("charlie@test.com", "bob@test.com", "alice@test.com");
    }

    [Fact]
    public async Task SortUsers_ThrowsValidationException_ForUnknownSortKey()
    {
        await SeedUsersAsync();

        var act = () => _context.Users.AsNoTracking().SortUsers(new SortRequest("unknown", "asc")).ToList();

        act.Should().Throw<ValidationException>()
            .WithMessage("*sortBy must be one of: nick, firstname, lastname, email*");
    }

    [Fact]
    public async Task SortUsers_TieBreaksById()
    {
        _context.Users.AddRange(
            new User { PublicId = Guid.NewGuid(), PublicNick = "same", FirstName = "First", LastName = "Last", Email = "same@test.com", PasswordHash = "hash" },
            new User { PublicId = Guid.NewGuid(), PublicNick = "same", FirstName = "First", LastName = "Last", Email = "same@test.com", PasswordHash = "hash" });
        await _context.SaveChangesAsync();

        var result = _context.Users.AsNoTracking().SortUsers(new SortRequest("nick", "asc")).ToList();

        result.Should().HaveCount(2);
        result[0].Id.Should().BeLessThan(result[1].Id);
    }

    [Fact]
    public async Task SortUsers_NullSortKey_DefaultsToNick()
    {
        await SeedUsersAsync();

        var result = _context.Users.AsNoTracking().SortUsers(new SortRequest(null, null)).ToList();

        result.Select(x => x.PublicNick).Should().ContainInOrder("alice", "bob", "charlie");
    }

    [Fact]
    public async Task SortUsers_EmptySortKey_DefaultsToNick()
    {
        await SeedUsersAsync();

        var result = _context.Users.AsNoTracking().SortUsers(new SortRequest("", "")).ToList();

        result.Select(x => x.PublicNick).Should().ContainInOrder("alice", "bob", "charlie");
    }

    [Fact]
    public void AllowedSortKeys_ContainsExpectedKeys()
    {
        UserSortMap.AllowedSortKeys.Should().BeEquivalentTo("nick", "firstname", "lastname", "email");
    }
}
