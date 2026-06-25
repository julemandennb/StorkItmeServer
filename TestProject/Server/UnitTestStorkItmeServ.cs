using Microsoft.EntityFrameworkCore;
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

        [Fact]
        public void TestGet()
        {
            using var context = _setDataBaseUp.Up("Get");

            var service = new StorkItmeServ(null, context);

            var storkItme = service.Get(2);
            var expected = context.StorkItme.FirstOrDefault(x => x.Id == 2);

            Assert.NotNull(storkItme);
            Assert.NotNull(expected);

            Assert.Equal(expected!.Id, storkItme!.Id);
        }

        [Fact]
        public void TestGet_ReturnsNull_WhenItemDoesNotExist()
        {
            using var context = _setDataBaseUp.Up("Get_NotFound");

            var service = new StorkItmeServ(null, context);

            var result = service.Get(99999);

            Assert.Null(result);
        }

        [Fact]
        public void TestGetFromItemNumber()
        {
            using var context = _setDataBaseUp.Up("GetFromItemNumber");

            var service = new StorkItmeServ(null, context);

            var expected = context.StorkItme.First();

            expected.ItemNumber = "ABC123";
            context.SaveChanges();

            var result = service.GetFromItemNumber("ABC123");

            Assert.NotNull(result);
            Assert.Equal(expected.Id, result!.Id);
        }

        [Fact]
        public void TestGetFromEAN()
        {
            using var context = _setDataBaseUp.Up("GetFromEAN");

            var service = new StorkItmeServ(null, context);

            var expected = context.StorkItme.First();

            expected.EAN = "EAN999";
            context.SaveChanges();

            var result = service.GetFromEAN("EAN999");

            Assert.NotNull(result);
            Assert.Equal(expected.Id, result!.Id);
        }

        [Fact]
        public void TestCreate_ReturnsEntityWithId()
        {
            using var context = _setDataBaseUp.Up("Create_Returns");

            var service = new StorkItmeServ(null, context);

            var item = new StorkItme
            {
                UserGroupId = 1,
                Name = "test item",
                Description = "desc",
                Type = "type",
                BestBy = DateTime.UtcNow,
                Stork = 1
            };

            var result = service.Create(item);

            Assert.NotNull(result);
            Assert.True(result!.Id > 0);
        }

        [Fact]
        public void TestGetAll()
        {
            using var context = _setDataBaseUp.Up("GetAll");

            var service = new StorkItmeServ(null, context);

            var result = service.GetAll();

            Assert.NotNull(result);
            Assert.Equal(_setDataBaseUp.StorkItmes().Count, result.Count);
        }

        [Fact]
        public void TestGetAll7DaysBeforeBestBy()
        {
            using var context = _setDataBaseUp.Up("GetAll7DaysBeforeBestBy");

            var service = new StorkItmeServ(null, context);

            var result = service.GetAll7DaysBeforeBestBy().ToList();

            Assert.Single(result);
            Assert.Equal("den har id 4", result[0].Name);
        }

        [Fact]
        public void TestGetAllAfterBestBy()
        {
            using var context = _setDataBaseUp.Up("GetAll7DaysBeforeBestBy");

            var service = new StorkItmeServ(null, context);

            var result = service.GetAllAfterBestBy().ToList();

            Assert.Single(result);
            Assert.Equal("den har id 3", result[0].Name);
        }

        [Fact]
        public void TestCreate()
        {
            using var context = _setDataBaseUp.Up("Create");

            var service = new StorkItmeServ(null, context);

            var before = context.StorkItme.Count();

            var item = new StorkItme
            {
                UserGroupId = 1,
                Name = "den har er add",
                Stork = 1,
                BestBy = DateTime.UtcNow,
                Description = "den har er add",
                Type = "fefs"
            };

            service.Create(item);

            var after = context.StorkItme.Count();

            Assert.Equal(before + 1, after);
        }

        [Fact]
        public void TestCreateWithoutSave()
        {
            using var context = _setDataBaseUp.Up("CreateWithoutSave");

            var service = new StorkItmeServ(null, context);

            var before = context.StorkItme.Count();

            var item = new StorkItme
            {
                UserGroupId = 1,
                Name = "den har er add",
                Stork = 1,
                BestBy = DateTime.UtcNow,
                Description = "den har er add",
                Type = "fefs"
            };

            service.CreateWithoutSave(item);

            Assert.Equal(before, context.StorkItme.Count());

            context.SaveChanges();

            Assert.Equal(before + 1, context.StorkItme.Count());
        }

        [Fact]
        public void TestUpdate()
        {
            using var context = _setDataBaseUp.Up("Updata");

            var service = new StorkItmeServ(null, context);

            var item = service.Get(1);
            var originalName = item!.Name;

            item.Name = "updated name";

            service.Update(item);

            var updated = service.Get(1);

            Assert.NotEqual(originalName, updated!.Name);
        }

        [Fact]
        public void TestUpdateWithoutSave()
        {
            using var context = _setDataBaseUp.Up("TestUpdateWithoutSave");

            var service = new StorkItmeServ(null, context);

            var item = service.Get(1);
            var original = item!.Name;

            item.Name = "Updated Name";

            service.UpdateWithoutSave(item);

            var notSaved = context.StorkItme.AsNoTracking().First(x => x.Id == 1);
            Assert.Equal(original, notSaved.Name);

            context.SaveChanges();

            var saved = context.StorkItme.AsNoTracking().First(x => x.Id == 1);
            Assert.Equal("Updated Name", saved.Name);
        }

        [Fact]
        public void TestDelete()
        {
            using var context = _setDataBaseUp.Up("Delete");

            var service = new StorkItmeServ(null, context);

            var before = context.StorkItme.Count();

            var item = service.Get(2);
            service.Delete(item!);

            var after = context.StorkItme.Count();

            Assert.Equal(before - 1, after);
        }

        [Fact]
        public void TestRemoveRange()
        {
            using var context = _setDataBaseUp.Up("RemoveRange");

            var service = new StorkItmeServ(null, context);

            var before = context.StorkItme.Count();

            var items = context.StorkItme.Where(x => x.Id > 3).ToList();

            service.RemoveRange(items);

            var after = context.StorkItme.Count();

            Assert.Equal(before - items.Count, after);
        }

        [Fact]
        public void TestDelete_RemovesItem()
        {
            using var context = _setDataBaseUp.Up("Delete_Check");

            var service = new StorkItmeServ(null, context);

            var item = context.StorkItme.First();

            var id = item.Id;

            service.Delete(item);

            var deleted = context.StorkItme.FirstOrDefault(x => x.Id == id);

            Assert.Null(deleted);
        }

        [Fact]
        public void TestRemoveRange_RemovesMultiple()
        {
            using var context = _setDataBaseUp.Up("RemoveRange_Multi");

            var service = new StorkItmeServ(null, context);

            var items = context.StorkItme.Take(2).ToList();
            var before = context.StorkItme.Count();

            service.RemoveRange(items);

            var after = context.StorkItme.Count();

            Assert.Equal(before - 2, after);
        }

        [Fact]
        public void TestUpdateWithoutSave_DoesNotPersistUntilSave()
        {
            using var context = _setDataBaseUp.Up("UpdateWithoutSave");

            var service = new StorkItmeServ(null, context);

            var item = service.Get(1);
            var original = item!.Name;

            item.Name = "changed";

            service.UpdateWithoutSave(item);

            var stillOriginal = context.StorkItme.AsNoTracking().First(x => x.Id == 1);

            Assert.Equal(original, stillOriginal.Name);

            context.SaveChanges();

            var updated = context.StorkItme.AsNoTracking().First(x => x.Id == 1);

            Assert.Equal("changed", updated.Name);
        }
    }
}