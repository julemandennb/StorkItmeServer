using Microsoft.EntityFrameworkCore;
using StorkItmeServer.Database;
using StorkItmeServer.Model;
using StorkItmeServer.Server;

namespace TestProject
{
    public class UnitTestUserGroupServ
    {
        private SetDataBaseUp _setDataBaseUp;

        public UnitTestUserGroupServ()
        {
            _setDataBaseUp = new SetDataBaseUp("UserGroupServ");
        }

        [Fact]
        public void TestGet()
        {

            using (var context = _setDataBaseUp.Up("Get"))
            {
                UserGroupServ userGroupServ = MakeUserGroupServ(context);
                UserGroup userGroup = userGroupServ.Get(2);
                Assert.NotNull(userGroup);
                UserGroup userGroupCehek = context.UserGroup.FirstOrDefault(x => x.Id == 2);
                Assert.NotNull(userGroupCehek);
                Assert.Equal(userGroupCehek, userGroup);

            }

        }

        [Fact]
        public void TestGetAll()
        {

            using (var context = _setDataBaseUp.Up("GetAll"))
            {
                UserGroupServ userGroupServ = MakeUserGroupServ(context);

                IQueryable<UserGroup> UserGroups = userGroupServ.GetAll();
                Assert.NotNull(UserGroups);

                Assert.Equal(_setDataBaseUp.UserGroups().Count(), UserGroups.Count());
            }

        }

        [Fact]
        public void TestCreate()
        {

            using (var context = _setDataBaseUp.Up("Create"))
            {
                UserGroupServ userGroupServ = MakeUserGroupServ(context);
                int checkNr = _setDataBaseUp.UserGroups().Count();
                int Nr = context.UserGroup.Count();
                Assert.Equal(checkNr, Nr);

                UserGroup userGroup = new UserGroup() { Name = "add", Color = "#fff" };
                userGroupServ.Create(userGroup);

                checkNr = _setDataBaseUp.UserGroups().Count() + 1;
                Nr = context.UserGroup.Count();

                Assert.Equal(checkNr, Nr);
            }

        }

        [Fact]
        public void TestCreateWithoutSave()
        {

            using (var context = _setDataBaseUp.Up("CreateWithoutSave"))
            {
                UserGroupServ userGroupServ = MakeUserGroupServ(context);

                int checkNr = _setDataBaseUp.UserGroups().Count();
                int Nr = context.UserGroup.Count();
                Assert.Equal(checkNr, Nr);

                UserGroup userGroup = new UserGroup() { Name = "add", Color = "#fff" };
                userGroupServ.CreateWithoutSave(userGroup);
                Nr = context.UserGroup.Count();

                Assert.Equal(checkNr, Nr);
                context.SaveChanges();
                Nr = context.UserGroup.Count();
                checkNr = _setDataBaseUp.UserGroups().Count() + 1;

                Assert.Equal(checkNr, Nr);

            }

        }

        [Fact]
        public void TestUpdata()
        {

            using (var context = _setDataBaseUp.Up("Updata"))
            {
                UserGroupServ userGroupServ = MakeUserGroupServ(context);

                string NameCheck = userGroupServ.Get(1).Name;

                UserGroup userGroup = userGroupServ.Get(1);

                Assert.Equal(NameCheck, userGroup.Name);

                userGroup.Name = "Ny name";

                userGroupServ.Updata(userGroup);

                userGroup = userGroupServ.Get(1);

                Assert.NotEqual(NameCheck, userGroup.Name);
            }

        }

        [Fact]
        public void TestUpdateWithoutSave()
        {

            using (var context = _setDataBaseUp.Up("TestUpdateWithoutSave"))
            {
                UserGroupServ userGroupServ = MakeUserGroupServ(context);

                var originalItem = userGroupServ.Get(1);
                string originalName = originalItem.Name;

                Assert.Equal("den har id 1", originalName);

                originalItem.Name = "Updated Name";
                userGroupServ.UpdateWithoutSave(originalItem);

                var updatedItemWithoutSave = context.UserGroup.AsNoTracking().FirstOrDefault(x => x.Id == 1);
                Assert.Equal("den har id 1", updatedItemWithoutSave.Name); // Name should still be the original in the database

                context.SaveChanges();

                var updatedItemWithSave = context.UserGroup.AsNoTracking().FirstOrDefault(x => x.Id == 1);
                Assert.Equal("Updated Name", updatedItemWithSave.Name); // Name should now be updated in the database
            }
        }

        [Fact]
        public void TestDelete()
        {

            using (var context = _setDataBaseUp.Up("Delete"))
            {
                UserGroupServ userGroupServ = MakeUserGroupServ(context);

                int checkNr = _setDataBaseUp.UserGroups().Count();
                int nr = context.UserGroup.Count();

                Assert.Equal(checkNr, nr);

                UserGroup userGroup = userGroupServ.Get(2);

                userGroupServ.Delete(userGroup);
                checkNr--;
                nr = context.UserGroup.Count();

                Assert.Equal(checkNr, nr);
            }

        }

        [Fact]
        public void TestDeleteWithoutSave()
        {

            using (var context = _setDataBaseUp.Up("DeleteWithoutSave"))
            {
                UserGroupServ userGroupServ = MakeUserGroupServ(context);

                int checkNr = _setDataBaseUp.UserGroups().Count();
                int nr = context.UserGroup.Count();

                Assert.Equal(checkNr, nr);

                UserGroup userGroup = userGroupServ.Get(2);

                userGroupServ.DeleteWithoutSave(userGroup);

                nr = context.UserGroup.Count();

                Assert.Equal(checkNr, nr);

                context.SaveChanges();
                checkNr--;
                nr = context.UserGroup.Count();

                Assert.Equal(checkNr, nr);

            }

        }

        private UserGroupServ MakeUserGroupServ(DataContext context)
        {
            StorkItmeServ storkItmeServ = new StorkItmeServ(null, context);
            UserGroupServ userGroupServ = new UserGroupServ(null, context, storkItmeServ);

            return userGroupServ;
        }

    }
}
