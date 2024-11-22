using Microsoft.EntityFrameworkCore;
using StorkItmeServer.Model;
using StorkItmeServer.Server;


namespace TestProject
{
    public class UnitTestStorkItmeServ
    {

        private SetDataBaseUp _setDataBaseUp;

        public UnitTestStorkItmeServ() {

            _setDataBaseUp = new SetDataBaseUp("StorkItmeServ");


        }



        [Fact]
        public void TestGet()
        {
            
            using (var context = _setDataBaseUp.Up("Get"))
            {
                StorkItmeServ storkItmeServ = new StorkItmeServ(null,context);
                StorkItme storkItme = storkItmeServ.Get(2);
                Assert.NotNull(storkItme);
                StorkItme StorkItmeCheck = context.StorkItme.FirstOrDefault(x => x.Id == 2);
                Assert.NotNull(StorkItmeCheck);

                Assert.Equal(StorkItmeCheck,storkItme);
            }

        }

        [Fact]
        public void TestGetAll()
        {

            using (var context = _setDataBaseUp.Up("GetAll"))
            {
                StorkItmeServ storkItmeServ = new StorkItmeServ(null, context);
                IQueryable<StorkItme> storkItmes = storkItmeServ.GetAll();
                Assert.NotNull(storkItmes);
     
                Assert.Equal(_setDataBaseUp.StorkItmes().Count(),storkItmes.Count());
            }

        }

        [Fact]
        public void TestCreate()
        {

            using (var context = _setDataBaseUp.Up("Create"))
            {
                StorkItmeServ storkItmeServ = new StorkItmeServ(null, context);

                int checkNr = _setDataBaseUp.StorkItmes().Count();
                int Nr = context.StorkItme.Count();
                Assert.Equal(checkNr, Nr);

                StorkItme storkItme = new StorkItme() { UserGroupId = 1, Name = "den har er add", Stork = 1, BestBy = new DateTime(), Description = "den har er add", Type = "fefs", ImgUrl = "" };
                storkItmeServ.Create(storkItme);
                checkNr = _setDataBaseUp.StorkItmes().Count() + 1;
                Nr = context.StorkItme.Count();

                Assert.Equal(checkNr, Nr);
            }

        }

        [Fact]
        public void TestCreateWithoutSave()
        {

            using (var context = _setDataBaseUp.Up("CreateWithoutSave"))
            {
                StorkItmeServ storkItmeServ = new StorkItmeServ(null, context);

                int checkNr = _setDataBaseUp.StorkItmes().Count();
                int Nr = context.StorkItme.Count();
                Assert.Equal(checkNr, Nr);

                StorkItme storkItme = new StorkItme() { UserGroupId = 1, Name = "den har er add", Stork = 1, BestBy = new DateTime(), Description = "den har er add", Type = "fefs", ImgUrl = "" };
                storkItmeServ.CreateWithoutSave(storkItme);
                Nr = context.StorkItme.Count();

                Assert.Equal(checkNr, Nr);
                context.SaveChanges();
                Nr = context.StorkItme.Count();
                checkNr = _setDataBaseUp.StorkItmes().Count() + 1;

                Assert.Equal(checkNr, Nr);

            }

        }

        [Fact]
        public void TestUpdata()
        {

            using (var context = _setDataBaseUp.Up("Updata"))
            {
                StorkItmeServ storkItmeServ = new StorkItmeServ(null, context);

                string storkItmeCheck = storkItmeServ.Get(1).Name;

                StorkItme storkItmeUpdata = storkItmeServ.Get(1);

                Assert.Equal(storkItmeCheck, storkItmeUpdata.Name);

                storkItmeUpdata.Name = "updata name";

                storkItmeServ.Updata(storkItmeUpdata);

                storkItmeUpdata = storkItmeServ.Get(1);

                Assert.NotEqual(storkItmeCheck, storkItmeUpdata.Name);
            }

        }

        [Fact]
        public void TestUpdateWithoutSave()
        {
           
            using (var context = _setDataBaseUp.Up("TestUpdateWithoutSave"))
            {
                var storkItmeServ = new StorkItmeServ(null, context);

                var originalItem = storkItmeServ.Get(1);
                string originalName = originalItem.Name;

                Assert.Equal("den har id 1", originalName);

                originalItem.Name = "Updated Name";
                storkItmeServ.UpdateWithoutSave(originalItem);

                var updatedItemWithoutSave = context.StorkItme.AsNoTracking().FirstOrDefault(x => x.Id == 1);
                Assert.Equal("den har id 1", updatedItemWithoutSave.Name); // Name should still be the original in the database

                context.SaveChanges();

                var updatedItemWithSave = context.StorkItme.AsNoTracking().FirstOrDefault(x => x.Id == 1);
                Assert.Equal("Updated Name", updatedItemWithSave.Name); // Name should now be updated in the database
            }
        }

        [Fact]
        public void TestDelete()
        {

            using (var context = _setDataBaseUp.Up("Delete"))
            {
                var storkItmeServ = new StorkItmeServ(null, context);

                int checkNr = _setDataBaseUp.StorkItmes().Count();
                int nr = context.StorkItme.Count();

                Assert.Equal(checkNr, nr);

                StorkItme storkItme = storkItmeServ.Get(2);

                storkItmeServ.Delete(storkItme);
                checkNr--;
                nr = context.StorkItme.Count();

                Assert.Equal(checkNr, nr);
            }

        }

        [Fact]
        public void TestDeleteWithoutSave()
        {

            using (var context = _setDataBaseUp.Up("DeleteWithoutSave"))
            {
                var storkItmeServ = new StorkItmeServ(null, context);

                int checkNr = _setDataBaseUp.StorkItmes().Count();
                int nr = context.StorkItme.Count();

                Assert.Equal(checkNr, nr);

                StorkItme storkItme = storkItmeServ.Get(2);

                storkItmeServ.DeleteWithoutSave(storkItme);
                
                nr = context.StorkItme.Count();

                Assert.Equal(checkNr, nr);

                context.SaveChanges();
                checkNr--;
                nr = context.StorkItme.Count();

                Assert.Equal(checkNr, nr);

            }

        }

        [Fact]
        public void TestRemoveRange()
        {

            using (var context = _setDataBaseUp.Up("RemoveRange"))
            {
                var storkItmeServ = new StorkItmeServ(null, context);

                int checkNr = _setDataBaseUp.StorkItmes().Count();
                int nr = context.StorkItme.Count();
                Assert.Equal(checkNr, nr);

                List<StorkItme> storkItmes = context.StorkItme.Where(x => x.Id > 3).ToList();

                storkItmeServ.RemoveRange(storkItmes);
                checkNr -= storkItmes.Count();
                nr = context.StorkItme.Count();

                Assert.Equal(checkNr, nr);
            }

        }

        [Fact]
        public void TestRemoveRangeWithoutSave()
        {

            using (var context = _setDataBaseUp.Up("RemoveRangeWithoutSave"))
            {
                var storkItmeServ = new StorkItmeServ(null, context);

                int checkNr = _setDataBaseUp.StorkItmes().Count();
                int nr = context.StorkItme.Count();
                Assert.Equal(checkNr, nr);

                List<StorkItme> storkItmes = context.StorkItme.Where(x => x.Id > 3).ToList();

                storkItmeServ.RemoveRangeWithoutSave(storkItmes);
                nr = context.StorkItme.Count();

                Assert.Equal(checkNr, nr);

                context.SaveChanges();

                checkNr -= storkItmes.Count();
                nr = context.StorkItme.Count();

                Assert.Equal(checkNr, nr);

            }

        }
    }
}