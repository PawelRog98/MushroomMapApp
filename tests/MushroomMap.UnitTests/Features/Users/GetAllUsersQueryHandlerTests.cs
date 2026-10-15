using FluentAssertions;
using Moq;
using MushroomMap.UnitTests.Common.Database;
using MushroomMapApp.Domain.Data;
using MushroomMapApp.Domain.Entities;
using MushroomMapApp.Domain.Interfaces;
using MushroomMapApp.Features.Common.Pagination;
using MushroomMapApp.Features.Users.GetAllUsers;
using Xunit;

namespace MushroomMap.UnitTests.Features.Users;

public class GetAllUsersQueryHandlerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly GetAllUsersQueryHandler _handler;

    public GetAllUsersQueryHandlerTests()
    {
        _context = TestDbContextFactory.Create();

        var permissionService = new Mock<IPermissionService>();
        permissionService
            .Setup(x => x.GetPermissions(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HashSet<string>());

        _handler = new GetAllUsersQueryHandler(_context, permissionService.Object);
    }

    public void Dispose() => _context.Dispose();

    private async Task<long> SeedRoleAsync()
    {
        var role = new Role { Name = "User" };
        _context.Roles.Add(role);
        await _context.SaveChangesAsync();
        return role.Id;
    }

    private static User CreateUser(long roleId, string nick, string email)
    {
        return new User
        {
            PublicId = Guid.NewGuid(),
            PublicNick = nick,
            FirstName = "first",
            LastName = "last",
            Email = email,
            PasswordHash = "hash",
            RoleId = roleId
        };
    }

    private static PaginationRequest Paging(int page = 1, int pageSize = 20) => new(page, pageSize);

    [Fact]
    public async Task Handle_WithoutSort_OrdersByPublicNickAscending()
    {
        var roleId = await SeedRoleAsync();
        _context.Users.AddRange(
            CreateUser(roleId, "charlie", "charlie@test.com"),
            CreateUser(roleId, "alice", "alice@test.com"),
            CreateUser(roleId, "bob", "bob@test.com"));
        await _context.SaveChangesAsync();

        var result = await _handler.Handle(
            new GetAllUsersQuery(Paging(), new SortRequest()), CancellationToken.None);

        result.items.Select(x => x.PublicNick).Should().ContainInOrder("alice", "bob", "charlie");
        result.TotalCount.Should().Be(3);
        result.TotalPages.Should().Be(1);
    }

    [Fact]
    public async Task Handle_SortsByEmailDescending_AndTieBreaksByIdAscending()
    {
        var roleId = await SeedRoleAsync();
        var firstSame = CreateUser(roleId, "first-same", "same@test.com");
        var secondSame = CreateUser(roleId, "second-same", "same@test.com");
        var other = CreateUser(roleId, "other", "zzz@test.com");
        _context.Users.AddRange(firstSame, secondSame, other);
        await _context.SaveChangesAsync();

        var result = await _handler.Handle(
            new GetAllUsersQuery(Paging(), new SortRequest("email", "desc")), CancellationToken.None);

        var ids = result.items.Select(x => x.PublicId).ToList();
        ids[0].Should().Be(other.PublicId);
        ids[1].Should().Be(firstSame.PublicId, "ties are broken by Id ascending even when the key sorts desc");
        ids[2].Should().Be(secondSame.PublicId);
    }

    [Fact]
    public async Task Handle_SortedPages_AreDisjointAndCoverAllRows()
    {
        var roleId = await SeedRoleAsync();
        for (var i = 0; i < 25; i++)
        {
            _context.Users.Add(CreateUser(roleId, $"nick-{i:D2}", $"user{i:D2}@test.com"));
        }
        await _context.SaveChangesAsync();

        var sort = new SortRequest("email", "desc");
        var page1 = await _handler.Handle(new GetAllUsersQuery(Paging(1, 10), sort), CancellationToken.None);
        var page2 = await _handler.Handle(new GetAllUsersQuery(Paging(2, 10), sort), CancellationToken.None);
        var page3 = await _handler.Handle(new GetAllUsersQuery(Paging(3, 10), sort), CancellationToken.None);

        page1.items.Should().HaveCount(10);
        page2.items.Should().HaveCount(10);
        page3.items.Should().HaveCount(5);
        page1.TotalCount.Should().Be(25);
        page1.TotalPages.Should().Be(3);

        var allIds = page1.items.Concat(page2.items).Concat(page3.items).Select(x => x.PublicId).ToList();
        allIds.Should().OnlyHaveUniqueItems();
        allIds.Should().HaveCount(25);

        var emails = page1.items.Concat(page2.items).Concat(page3.items).Select(x => x.Email).ToList();
        emails.Should().BeInDescendingOrder();
    }

    [Fact]
    public async Task Handle_ReturnsEmptyPage_WhenPageIsOutOfRange()
    {
        var roleId = await SeedRoleAsync();
        _context.Users.Add(CreateUser(roleId, "only", "only@test.com"));
        await _context.SaveChangesAsync();

        var result = await _handler.Handle(
            new GetAllUsersQuery(Paging(99, 20), new SortRequest()), CancellationToken.None);

        result.items.Should().BeEmpty();
        result.TotalCount.Should().Be(1);
        result.TotalPages.Should().Be(1);
        result.HasNext.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_FiltersBySearchTerm_AcrossNickNameAndEmail()
    {
        var roleId = await SeedRoleAsync();
        var byNick = CreateUser(roleId, "mushroom-fan", "fan@test.com");
        var byFirstName = CreateUser(roleId, "nick-a", "a@test.com");
        byFirstName.FirstName = "Mushroom";
        var byLastName = CreateUser(roleId, "nick-b", "b@test.com");
        byLastName.LastName = "mushroom";
        var byEmail = CreateUser(roleId, "nick-c", "mushroom@test.com");
        var unrelated = CreateUser(roleId, "unrelated", "other@test.com");
        _context.Users.AddRange(byNick, byFirstName, byLastName, byEmail, unrelated);
        await _context.SaveChangesAsync();

        var result = await _handler.Handle(
            new GetAllUsersQuery(Paging(), new SortRequest(), new FiltersRequest("mushroom")),
            CancellationToken.None);

        result.items.Select(x => x.PublicId)
            .Should().BeEquivalentTo([byNick.PublicId, byFirstName.PublicId, byLastName.PublicId, byEmail.PublicId]);
        result.TotalCount.Should().Be(4);
    }

    [Fact]
    public async Task Handle_SearchIsCaseInsensitive_AndTrimsWhitespace()
    {
        var roleId = await SeedRoleAsync();
        var match = CreateUser(roleId, "Charlie", "charlie@test.com");
        var other = CreateUser(roleId, "bob", "bob@test.com");
        _context.Users.AddRange(match, other);
        await _context.SaveChangesAsync();

        var result = await _handler.Handle(
            new GetAllUsersQuery(Paging(), new SortRequest(), new FiltersRequest("  CHARLIE  ")),
            CancellationToken.None);

        result.items.Should().ContainSingle();
        result.items[0].PublicId.Should().Be(match.PublicId);
    }

    [Fact]
    public async Task Handle_ReturnsEmptyResult_WhenSearchMatchesNothing()
    {
        var roleId = await SeedRoleAsync();
        _context.Users.Add(CreateUser(roleId, "only", "only@test.com"));
        await _context.SaveChangesAsync();

        var result = await _handler.Handle(
            new GetAllUsersQuery(Paging(), new SortRequest(), new FiltersRequest("no-such-user")),
            CancellationToken.None);

        result.items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
    }

    [Fact]
    public async Task Handle_NullOrWhitespaceSearch_ReturnsAllUsers()
    {
        var roleId = await SeedRoleAsync();
        _context.Users.AddRange(
            CreateUser(roleId, "alice", "alice@test.com"),
            CreateUser(roleId, "bob", "bob@test.com"));
        await _context.SaveChangesAsync();

        var nullFilters = await _handler.Handle(
            new GetAllUsersQuery(Paging(), new SortRequest(), null), CancellationToken.None);
        var whitespaceFilters = await _handler.Handle(
            new GetAllUsersQuery(Paging(), new SortRequest(), new FiltersRequest("   ")), CancellationToken.None);

        nullFilters.TotalCount.Should().Be(2);
        whitespaceFilters.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_FilteredPagination_UsesFilteredTotalCount()
    {
        var roleId = await SeedRoleAsync();
        for (var i = 0; i < 5; i++)
        {
            _context.Users.Add(CreateUser(roleId, $"match-{i}", $"match{i}@test.com"));
        }
        _context.Users.Add(CreateUser(roleId, "other", "other@test.com"));
        await _context.SaveChangesAsync();

        var page1 = await _handler.Handle(
            new GetAllUsersQuery(Paging(1, 2), new SortRequest(), new FiltersRequest("match")),
            CancellationToken.None);
        var page3 = await _handler.Handle(
            new GetAllUsersQuery(Paging(3, 2), new SortRequest(), new FiltersRequest("match")),
            CancellationToken.None);

        page1.items.Should().HaveCount(2);
        page1.TotalCount.Should().Be(5);
        page1.TotalPages.Should().Be(3);
        page3.items.Should().HaveCount(1);
        page1.items.Concat(page3.items).Should().NotContain(x => x.Email == "other@test.com");
    }
}
