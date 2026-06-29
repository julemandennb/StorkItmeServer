using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StorkItmeServer.Database;
using StorkItmeServer.Model;
using StorkItmeServer.Server;

namespace TestProject.Server
{
    public class UnitTestUserGroupServ
    {
        private readonly SetDataBaseUp _setDataBaseUp;

        public UnitTestUserGroupServ()
        {
            _setDataBaseUp = new SetDataBaseUp("UserGroupServ");
        }

        private UserGroupServ CreateService(DataContext context)
            => new UserGroupServ(
                new LoggerFactory().CreateLogger<UserGroupServ>(),
                context,
                new StorkItmeServ(new LoggerFactory().CreateLogger<StorkItmeServ>(), context)
            );

        // ------------------------
        // GET
        // ------------------------

        [Fact]
        public async Task GetById_ReturnsCorrectEntity()
        {
            using var context = _setDataBaseUp.Up("Get");

            var service = CreateService(context);

            var expected = await context.UserGroup.FirstAsync();
            var result = service.Get(expected.Id);

            Assert.NotNull(result);
            Assert.Equal(expected.Id, result!.Id);
        }

        [Fact]
        public async Task GetById_ReturnsNull_WhenNotFound()
        {
            using var context = _setDataBaseUp.Up("Get_NotFound");

            var service = CreateService(context);

            var result = service.Get(999999);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetByUuid_ReturnsCorrectEntity()
        {
            using var context = _setDataBaseUp.Up("GetByUuid");

            var service = CreateService(context);

            var entity = await context.UserGroup.FirstAsync();

            var result = service.Get(entity.Uuid.ToString());

            Assert.NotNull(result);
            Assert.Equal(entity.Id, result!.Id);
        }

        [Fact]
        public async Task GetByUuid_ReturnsNull_WhenInvalidGuid()
        {
            using var context = _setDataBaseUp.Up("GetByUuid_Invalid");

            var service = CreateService(context);

            var result = service.Get("not-a-guid");

            Assert.Null(result);
        }

        // ------------------------
        // GET ALL
        // ------------------------

        [Fact]
        public async Task GetAll_ReturnsAllEntities()
        {
            using var context = _setDataBaseUp.Up("GetAll");

            var service = CreateService(context);

            var result = service.GetAll();

            Assert.NotNull(result);

            var expectedCount = await context.UserGroup.CountAsync();
            var actualCount = result!.Count();

            Assert.Equal(expectedCount, actualCount);
        }

        // ------------------------
        // CREATE
        // ------------------------

        [Fact]
        public async Task Create_ReturnsEntity()
        {
            using var context = _setDataBaseUp.Up("Create_Returns");

            var service = CreateService(context);

            var entity = new UserGroup
            {
                Name = "test group",
                Color = "#fff"
            };

            var result = service.Create(entity);

            Assert.NotNull(result);
            Assert.True(result!.Id > 0);
        }

        [Fact]
        public async Task Create_IncreasesCount()
        {
            using var context = _setDataBaseUp.Up("Create");

            var service = CreateService(context);

            var before = await context.UserGroup.CountAsync();

            service.Create(new UserGroup
            {
                Name = "created group",
                Color = "#000"
            });

            var after = await context.UserGroup.CountAsync();

            Assert.Equal(before + 1, after);
        }

        // ------------------------
        // UPDATE
        // ------------------------

        [Fact]
        public async Task Update_ChangesEntity()
        {
            using var context = _setDataBaseUp.Up("Update");

            var service = CreateService(context);

            var entity = await context.UserGroup.FirstAsync();
            var original = entity.Name;

            entity.Name = "updated name";

            var result = service.Updata(entity);

            var updated = await context.UserGroup.FirstAsync(x => x.Id == entity.Id);

            Assert.True(result);
            Assert.NotEqual(original, updated.Name);
        }

        [Fact]
        public async Task UpdateWithoutSave_DoesNotPersistUntilSaved()
        {
            using var context = _setDataBaseUp.Up("UpdateWithoutSave");

            var service = CreateService(context);

            var entity = await context.UserGroup.FirstAsync();
            var original = entity.Name;

            entity.Name = "temporary update";

            service.UpdateWithoutSave(entity);

            var beforeSave = await context.UserGroup.AsNoTracking().FirstAsync(x => x.Id == entity.Id);
            Assert.Equal(original, beforeSave.Name);

            await context.SaveChangesAsync();

            var afterSave = await context.UserGroup.AsNoTracking().FirstAsync(x => x.Id == entity.Id);
            Assert.Equal("temporary update", afterSave.Name);
        }

        // ------------------------
        // DELETE
        // ------------------------

        [Fact]
        public async Task Delete_RemovesEntity()
        {
            using var context = _setDataBaseUp.Up("Delete");

            var service = CreateService(context);

            var before = await context.UserGroup.CountAsync();

            var entity = await context.UserGroup.FirstAsync();

            var result = service.Delete(entity);

            var after = await context.UserGroup.CountAsync();

            Assert.True(result);
            Assert.Equal(before - 1, after);
        }

        [Fact]
        public async Task DeleteWithoutSave_RemovesAfterSave()
        {
            using var context = _setDataBaseUp.Up("DeleteWithoutSave");

            var service = CreateService(context);

            var before = await context.UserGroup.CountAsync();

            var entity = await context.UserGroup.FirstAsync();

            var result = service.DeleteWithoutSave(entity);

            Assert.True(result);
            Assert.Equal(before, context.UserGroup.Count());

            await context.SaveChangesAsync();

            var after = await context.UserGroup.CountAsync();

            Assert.Equal(before - 1, after);
        }
    }
}