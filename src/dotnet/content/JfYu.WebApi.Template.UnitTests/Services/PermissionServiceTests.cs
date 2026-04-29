using JfYu.WebApi.Template.Entity;
using JfYu.WebApi.Template.Model;
using JfYu.WebApi.Template.Model.Permission;
using JfYu.WebApi.Template.Services;
using JfYu.WebApi.Template.Services.Interfaces;
using JfYu.WebApi.Template.UnitTests.TestBase;
using Microsoft.Extensions.Logging.Abstractions;

namespace JfYu.WebApi.Template.UnitTests.Services
{
    public class PermissionServiceTests
    {
        private static PermissionService CreateService(out (Entity.AppDbContext ctx, JfYu.Data.Context.ReadonlyDBContext<Entity.AppDbContext> ro) bag)
        {
            bag = DbContextFactory.CreateInMemory();
            return new PermissionService(bag.ctx, bag.ro, NullLogger<Permission>.Instance);
        }

        // ─── GetPagedAsync ────────────────────────────────────────────────────

        [Fact]
        public async Task GetPagedAsync_NoFilter_ReturnsAllPermissions()
        {
            var svc = CreateService(out var bag);
            bag.ctx.Permissions.AddRange(
                new Permission { Code = "a", Name = "A" },
                new Permission { Code = "b", Name = "B" },
                new Permission { Code = "c", Name = "C" });
            await bag.ctx.SaveChangesAsync();

            var result = await svc.GetPagedAsync(new QueryRequest { PageIndex = 1, PageSize = 10 });

            result.Total.Should().Be(3);
        }

        [Fact]
        public async Task GetPagedAsync_FilterBySearchKeyCode_ReturnsMatches()
        {
            var svc = CreateService(out var bag);
            bag.ctx.Permissions.AddRange(
                new Permission { Code = "user:get", Name = "Get User" },
                new Permission { Code = "role:edit", Name = "Edit Role" });
            await bag.ctx.SaveChangesAsync();

            var result = await svc.GetPagedAsync(new QueryRequest { SearchKey = "user", PageIndex = 1, PageSize = 10 });

            result.Total.Should().Be(1);
            result.Items.First().Code.Should().Be("user:get");
        }

        [Fact]
        public async Task GetPagedAsync_FilterBySearchKeyName_ReturnsMatches()
        {
            var svc = CreateService(out var bag);
            bag.ctx.Permissions.AddRange(
                new Permission { Code = "a", Name = "Alpha" },
                new Permission { Code = "b", Name = "Beta" });
            await bag.ctx.SaveChangesAsync();

            var result = await svc.GetPagedAsync(new QueryRequest { SearchKey = "Beta", PageIndex = 1, PageSize = 10 });

            result.Total.Should().Be(1);
        }

        [Fact]
        public async Task GetPagedAsync_FilterByStatus_ReturnsMatches()
        {
            var svc = CreateService(out var bag);
            bag.ctx.Permissions.AddRange(
                new Permission { Code = "active", Name = "Active", Status = 1 },
                new Permission { Code = "inactive", Name = "Inactive", Status = 0 });
            await bag.ctx.SaveChangesAsync();

            var result = await svc.GetPagedAsync(new QueryRequest { Status = 1, PageIndex = 1, PageSize = 10 });

            result.Total.Should().Be(1);
            result.Items.First().Code.Should().Be("active");
        }

        [Fact]
        public async Task GetPagedAsync_OrderedBySortThenId()
        {
            var svc = CreateService(out var bag);
            bag.ctx.Permissions.AddRange(
                new Permission { Code = "z", Name = "Z", Sort = 10 },
                new Permission { Code = "a", Name = "A", Sort = 1 },
                new Permission { Code = "m", Name = "M", Sort = 5 });
            await bag.ctx.SaveChangesAsync();

            var result = await svc.GetPagedAsync(new QueryRequest { PageIndex = 1, PageSize = 10 });

            result.Items[0].Code.Should().Be("a");
            result.Items[1].Code.Should().Be("m");
            result.Items[2].Code.Should().Be("z");
        }

        [Fact]
        public async Task GetPagedAsync_Paginates_Correctly()
        {
            var svc = CreateService(out var bag);
            for (int i = 1; i <= 8; i++)
                bag.ctx.Permissions.Add(new Permission { Code = $"p{i}", Name = $"P{i}" });
            await bag.ctx.SaveChangesAsync();

            var result = await svc.GetPagedAsync(new QueryRequest { PageIndex = 1, PageSize = 5 });

            result.Items.Count.Should().Be(5);
            result.Total.Should().Be(8);
        }

        [Fact]
        public async Task GetPagedAsync_FiltersByStartAndEndTime()
        {
            var svc = CreateService(out var bag);
            var now = System.DateTime.UtcNow;
            bag.ctx.Permissions.AddRange(
                new Permission { Code = "old", Name = "old", CreatedTime = now.AddDays(-10) },
                new Permission { Code = "mid", Name = "mid", CreatedTime = now.AddDays(-2) },
                new Permission { Code = "new", Name = "new", CreatedTime = now });
            await bag.ctx.SaveChangesAsync();

            var result = await svc.GetPagedAsync(new QueryRequest
            {
                StartTime = now.AddDays(-5),
                EndTime = now.AddDays(-1),
                PageIndex = 1,
                PageSize = 10,
            });

            result.Total.Should().Be(1);
            result.Items[0].Code.Should().Be("mid");
        }

        // ─── SyncAsync ────────────────────────────────────────────────────────

        [Fact]
        public void SyncAsync_SeedsPermissionsFromControllerAttributes()
        {
            var svc = CreateService(out var bag);

            svc.SyncAsync();

            // Parent menu ("system") shared by RBAC controllers should be seeded.
            bag.ctx.Permissions.Should().Contain(p => p.Code == JfYu.WebApi.Template.Constants.PermissionCodes.System);
            bag.ctx.Permissions.Count().Should().BeGreaterThan(1);
        }

        [Fact]
        public void SyncAsync_IsIdempotent()
        {
            var svc = CreateService(out var bag);

            svc.SyncAsync();
            var firstCount = bag.ctx.Permissions.Count();
            svc.SyncAsync();
            var secondCount = bag.ctx.Permissions.Count();

            secondCount.Should().Be(firstCount);
        }
    }
}
