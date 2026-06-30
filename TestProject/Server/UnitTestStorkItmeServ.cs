using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StorkItmeServer.Database;
using StorkItmeServer.Model;
using StorkItmeServer.Server;

namespace TestProject.Server
{
    public class UnitTestStorkItmeServ
    {
        private readonly SetDataBaseUp _setDataBaseUp;

        public UnitTestStorkItmeServ()
        {
            _setDataBaseUp = new SetDataBaseUp("StorkItmeServ");
        }

        private StorkItmeServ CreateService(DataContext context)
            => new StorkItmeServ(
                new LoggerFactory().CreateLogger<StorkItmeServ>(),
                context
            );

        // ------------------------
        // GET
        // ------------------------

        [Fact]
        public async Task GetById_ReturnsCorrectEntity()
        {
            using var context = _setDataBaseUp.Up("Get");

            var service = CreateService(context);

            var expected = await context.StorkItme.FirstAsync();
            var result = await service.GetAsync(expected.Id);

            Assert.NotNull(result);
            Assert.Equal(expected.Id, result!.Id);
        }

        [Fact]
        public async Task GetById_ReturnsNull_WhenNotFound()
        {
            using var context = _setDataBaseUp.Up("Get_NotFound");

            var service = CreateService(context);

            var result = await service.GetAsync(999999);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetByUuid_ReturnsCorrectEntity()
        {
            using var context = _setDataBaseUp.Up("GetByUuid");

            var service = CreateService(context);

            var entity = await context.StorkItme.FirstAsync();

            var result = await service.GetAsync(entity.Uuid.ToString());

            Assert.NotNull(result);
            Assert.Equal(entity.Id, result!.Id);
        }

        [Fact]
        public async Task GetByUuid_ReturnsNull_WhenInvalidGuid()
        {
            using var context = _setDataBaseUp.Up("GetByUuid_Invalid");

            var service = CreateService(context);

            var result = await service.GetAsync("not-a-guid");

            Assert.Null(result);
        }

        [Fact]
        public async Task GetFromItemNumber_ReturnsCorrectItem()
        {
            using var context = _setDataBaseUp.Up("GetFromItemNumber");

            var service = CreateService(context);

            var entity = await context.StorkItme.FirstAsync();
            entity.ItemNumber = "TEST-ITEM-123";
            await context.SaveChangesAsync();

            var result = await service.GetFromItemNumberAsync("TEST-ITEM-123");

            Assert.NotNull(result);
            Assert.Equal(entity.Id, result!.Id);
        }

        [Fact]
        public async Task GetFromEAN_ReturnsCorrectItem()
        {
            using var context = _setDataBaseUp.Up("GetFromEAN");

            var service = CreateService(context);

            var entity = await context.StorkItme.FirstAsync();
            entity.EAN = "EAN-TEST-999";
            await context.SaveChangesAsync();

            var result = await service.GetFromEANAsync("EAN-TEST-999");

            Assert.NotNull(result);
            Assert.Equal(entity.Id, result!.Id);
        }

        // ----------------------------
        // GET ALL
        // ----------------------------

        [Fact]
        public async Task GetAll_Returns_All_Items_When_No_Filter()
        {
            using var context = _setDataBaseUp.Up("GetAll_All");

            var service = CreateService(context);

            var result = await service.GetAllAsync();

            var expected = await context.StorkItme.CountAsync();

            Assert.Equal(expected, result.Count);
        }

        [Fact]
        public async Task GetAll_Filters_By_UserGroupId()
        {
            using var context = _setDataBaseUp.Up("GetAll_UserGroup");

            var service = CreateService(context);

            var groupId = await context.UserGroup
                .Select(x => x.Id)
                .FirstAsync();

            var result = await service.GetAllAsync(
                groupIds: new HashSet<int> { groupId }
            );

            Assert.NotEmpty(result);

            Assert.All(result, x =>
                Assert.Equal(groupId, x.UserGroupId)
            );
        }

        [Fact]
        public async Task GetAll_Filters_By_StorkItmeGroupId()
        {
            using var context = _setDataBaseUp.Up("GetAll_StorkGroup");

            var service = CreateService(context);

            var groupId = await context.StorkItmeGroup.OrderBy(x => x.Id).Select(x => x.Id).FirstAsync();

            var result = await service.GetAllAsync(
                storkItmeGroupIds: new HashSet<int> { groupId }
            );

            Assert.NotEmpty(result);

            Assert.All(result, x =>
                Assert.Equal(groupId, x.StorkItmeGroupId)
            );
        }

        [Fact]
        public async Task GetAll_Returns_Empty_When_No_Match()
        {
            using var context = _setDataBaseUp.Up("GetAll_NoMatch");

            var service = CreateService(context);

            var result = await service.GetAllAsync(
                groupIds: new HashSet<int> { 99999 }
            );

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAll_Uses_OR_Logic_Between_Groups()
        {
            using var context = _setDataBaseUp.Up("GetAll_OR");

            var service = CreateService(context);

            var userGroupId = await context.UserGroup.OrderBy(x => x.Id).Select(x => x.Id).FirstAsync();
            var storkGroupId = await context.StorkItmeGroup.OrderBy(x => x.Id).Select(x => x.Id).FirstAsync();

            var result = await service.GetAllAsync(
                groupIds: new HashSet<int> { userGroupId },
                storkItmeGroupIds: new HashSet<int> { storkGroupId }
            );

            Assert.NotEmpty(result);

            Assert.True(
                result.Any(x => x.UserGroupId == userGroupId) ||
                result.Any(x => x.StorkItmeGroupId == storkGroupId)
            );
        }

        // ----------------------------
        // GET NOT EXPIRED
        // ----------------------------

        [Fact]
        public async Task GetAllNotExpired_Returns_Future_Items_Only()
        {
            using var context = _setDataBaseUp.Up("NotExpired");

            var service = CreateService(context);

            var result = await service.GetAllNotExpiredAsync();

            Assert.All(result, x =>
                Assert.True(x.BestBy >= DateTime.UtcNow)
            );
        }

        [Fact]
        public async Task GetAllNotExpired_Excludes_Expired_Items()
        {
            using var context = _setDataBaseUp.Up("NotExpired_Exclude");

            var service = CreateService(context);

            var expired = context.StorkItme
                .Where(x => x.BestBy < DateTime.UtcNow)
                .ToList();

            var result = await service.GetAllNotExpiredAsync();

            Assert.DoesNotContain(result, x => expired.Any(e => e.Id == x.Id));
        }

        // ----------------------------
        // GET 7 DAYS BEFORE BEST BY
        // ----------------------------

        [Fact]
        public async Task GetAll7DaysBeforeBestBy_Returns_Items_Within_Range()
        {
            using var context = _setDataBaseUp.Up("7Days_Range");

            var service = CreateService(context);

            var now = DateTime.UtcNow;

            context.StorkItme.Add(new StorkItme
            {
                Name = "valid",
                UserGroupId = 1,
                Description = "Test",
                Type = "TestType",
                Stork = 1,
                BestBy = now.AddDays(3)
            });

            await context.SaveChangesAsync();

            var result = await service.GetAll7DaysBeforeBestByAsync();

            Assert.Contains(result, x => x.Name == "valid");
        }

        [Fact]
        public async Task GetAll7DaysBeforeBestBy_Excludes_OutOfRange_Items()
        {
            using var context = _setDataBaseUp.Up("7Days_Exclude");

            var service = CreateService(context);

            var now = DateTime.UtcNow;

            context.StorkItme.Add(new StorkItme
            {
                Name = "too late",
                Description = "desc",
                Type = "type",
                UserGroupId = 1,
                Stork = 1,
                BestBy = now.AddDays(20)
            });

            await context.SaveChangesAsync();

            var result = await service.GetAll7DaysBeforeBestByAsync();

            Assert.DoesNotContain(result, x => x.Name == "too late");
        }
    

        // ------------------------
        // CREATE
        // ------------------------

        [Fact]
        public async Task Create_ReturnsEntity()
        {
            using var context = _setDataBaseUp.Up("Create_Returns");

            var service = CreateService(context);

            var item = new StorkItme
            {
                UserGroupId = 1,
                Name = "test item",
                Description = "desc",
                Type = "type",
                BestBy = DateTime.UtcNow,
                Stork = 1
            };

            var result = await service.CreateAsync(item);

            Assert.NotNull(result);
            Assert.True(result!.Id > 0);
        }

        [Fact]
        public async Task Create_IncreasesCount()
        {
            using var context = _setDataBaseUp.Up("Create");

            var service = CreateService(context);

            var before = await context.StorkItme.CountAsync();

            await service.CreateAsync(new StorkItme
            {
                UserGroupId = 1,
                Name = "created item",
                Stork = 1,
                BestBy = DateTime.UtcNow,
                Description = "desc",
                Type = "type"
            });

            var after = await context.StorkItme.CountAsync();

            Assert.Equal(before + 1, after);
        }

        [Fact]
        public async Task Create_AssignsUuid()
        {
            using var context = _setDataBaseUp.Up("Create_Uuid");

            var service = CreateService(context);

            var result = await service.CreateAsync(new StorkItme
            {
                UserGroupId = 1,
                Name = "uuid test item",
                Stork = 1,
                BestBy = DateTime.UtcNow,
                Description = "desc",
                Type = "type"
            });

            Assert.NotNull(result);
            Assert.NotEqual(Guid.Empty, result!.Uuid);
        }

        // ------------------------
        // UPDATE
        // ------------------------

        [Fact]
        public async Task Update_ChangesEntity()
        {
            using var context = _setDataBaseUp.Up("Update");

            var service = CreateService(context);

            var entity = await context.StorkItme.FirstAsync();
            var original = entity.Name;

            entity.Name = "updated name";

            var result = await service.UpdateAsync(entity);

            var updated = await context.StorkItme.FirstAsync(x => x.Id == entity.Id);

            Assert.True(result);
            Assert.NotEqual(original, updated.Name);
        }

        // ------------------------
        // DELETE
        // ------------------------

        [Fact]
        public async Task Delete_RemovesEntity()
        {
            using var context = _setDataBaseUp.Up("Delete");

            var service = CreateService(context);

            var before = await context.StorkItme.CountAsync();

            var entity = await context.StorkItme.FirstAsync();

            var result = await service.DeleteAsync(entity.Id);

            var after = await context.StorkItme.CountAsync();

            Assert.True(result);
            Assert.Equal(before - 1, after);
        }

        [Fact]
        public async Task DeleteByUuid_RemovesEntity()
        {
            using var context = _setDataBaseUp.Up("DeleteByUuid");

            var service = CreateService(context);

            var entity = await context.StorkItme.FirstAsync();
            var before = await context.StorkItme.CountAsync();

            var result = await service.DeleteAsync(entity.Uuid.ToString());

            var after = await context.StorkItme.CountAsync();

            Assert.True(result);
            Assert.Equal(before - 1, after);
        }

        [Fact]
        public async Task DeleteByUuid_ReturnsFalse_WhenInvalidGuid()
        {
            using var context = _setDataBaseUp.Up("DeleteByUuid_Invalid");

            var service = CreateService(context);

            var result = await service.DeleteAsync("invalid-guid");

            Assert.False(result);
        }

        [Fact]
        public async Task RemoveRange_RemovesMultipleEntities()
        {
            using var context = _setDataBaseUp.Up("RemoveRange");

            var service = CreateService(context);

            var before = await context.StorkItme.CountAsync();

            var items = await context.StorkItme
                .Where(x => x.Id > 3)
                .ToListAsync();

            var result = await service.RemoveRangeAsync(items);

            var after = await context.StorkItme.CountAsync();

            Assert.True(result);
            Assert.Equal(before - items.Count, after);
        }
    }
}