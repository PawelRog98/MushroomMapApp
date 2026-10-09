using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MushroomMap.UnitTests.Common.Database;
using MushroomMapApp.Domain.Data;
using MushroomMapApp.Domain.Entities;
using MushroomMapApp.Features.Common.Pagination;
using Xunit;

namespace MushroomMap.UnitTests.Features.Common;

public class PaginatorTests : IDisposable
{
    private readonly AppDbContext _context;

    public PaginatorTests()
    {
        _context = TestDbContextFactory.Create();
    }

    public void Dispose() => _context.Dispose();

    private async Task SeedUsersAsync(int count)
    {
        for (var i = 0; i < count; i++)
        {
            _context.Users.Add(new User
            {
                PublicId = Guid.NewGuid(),
                PublicNick = $"user-{i:D3}",
                FirstName = "First",
                LastName = "Last",
                Email = $"user{i:D3}@test.com",
                PasswordHash = "hash"
            });
        }
        await _context.SaveChangesAsync();
    }

    [Fact]
    public async Task PaginateList_ReturnsCorrectPage()
    {
        await SeedUsersAsync(25);

        var query = _context.Users.AsNoTracking();
        var result = await query.PaginateList(new PaginationRequest(2, 10), q => q.OrderBy(x => x.PublicNick), CancellationToken.None);

        result.items.Should().HaveCount(10);
        result.TotalCount.Should().Be(25);
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(10);
        result.TotalPages.Should().Be(3);
        result.HasPrevious.Should().BeTrue();
        result.HasNext.Should().BeTrue();
    }

    [Fact]
    public async Task PaginateList_ReturnsLastPage()
    {
        await SeedUsersAsync(25);

        var query = _context.Users.AsNoTracking();
        var result = await query.PaginateList(new PaginationRequest(3, 10), q => q.OrderBy(x => x.PublicNick), CancellationToken.None);

        result.items.Should().HaveCount(5);
        result.TotalCount.Should().Be(25);
        result.HasPrevious.Should().BeTrue();
        result.HasNext.Should().BeFalse();
    }

    [Fact]
    public async Task PaginateList_ReturnsEmpty_WhenPageOutOfRange()
    {
        await SeedUsersAsync(5);

        var query = _context.Users.AsNoTracking();
        var result = await query.PaginateList(new PaginationRequest(10, 10), q => q.OrderBy(x => x.PublicNick), CancellationToken.None);

        result.items.Should().BeEmpty();
        result.TotalCount.Should().Be(5);
        result.HasPrevious.Should().BeTrue();
        result.HasNext.Should().BeFalse();
    }

    [Fact]
    public async Task PaginateList_ReturnsAll_WhenPageSizeLargerThanTotal()
    {
        await SeedUsersAsync(5);

        var query = _context.Users.AsNoTracking();
        var result = await query.PaginateList(new PaginationRequest(1, 100), q => q.OrderBy(x => x.PublicNick), CancellationToken.None);

        result.items.Should().HaveCount(5);
        result.TotalCount.Should().Be(5);
        result.TotalPages.Should().Be(1);
        result.HasPrevious.Should().BeFalse();
        result.HasNext.Should().BeFalse();
    }

    [Fact]
    public async Task PaginateList_AppliesOrderBy()
    {
        await SeedUsersAsync(5);

        var query = _context.Users.AsNoTracking();
        var result = await query.PaginateList(
            new PaginationRequest(1, 3),
            q => q.OrderByDescending(x => x.PublicNick),
            CancellationToken.None);

        result.items.Should().HaveCount(3);
        result.items.Should().BeInDescendingOrder(x => x.PublicNick);
    }

    [Fact]
    public async Task PaginateList_ReturnsEmpty_WhenNoData()
    {
        var query = _context.Users.AsNoTracking();
        var result = await query.PaginateList(new PaginationRequest(1, 10), q => q.OrderBy(x => x.PublicNick), CancellationToken.None);

        result.items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
        result.HasPrevious.Should().BeFalse();
        result.HasNext.Should().BeFalse();
    }
}
