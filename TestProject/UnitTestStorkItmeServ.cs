using Microsoft.EntityFrameworkCore;
using StorkItmeServer.Database;
using StorkItmeServer.Model;
using StorkItmeServer.Server;
using System;
using System.Xml.Linq;

namespace TestProject
{
    public class UnitTestStorkItmeServ
    {

        [Fact]
        public void TestGet()
        {
            
            using (var context = SetDataBaseUp("Get"))
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

            using (var context = SetDataBaseUp("GetAll"))
            {
                StorkItmeServ storkItmeServ = new StorkItmeServ(null, context);
                IQueryable<StorkItme> storkItmes = storkItmeServ.GetAll();
                Assert.NotNull(storkItmes);
     
                Assert.Equal(StorkItmes().Count(),storkItmes.Count());
            }

        }

        [Fact]
        public void TestCreate()
        {

            using (var context = SetDataBaseUp("Create"))
            {
                StorkItmeServ storkItmeServ = new StorkItmeServ(null, context);

                int checkNr = StorkItmes().Count();
                int Nr = context.StorkItme.Count();
                Assert.Equal(checkNr, Nr);

                StorkItme storkItme = new StorkItme() { UserGroupId = 1, Name = "den har er add", Stork = 1, BestBy = new DateTime(), Description = "den har er add", Type = "fefs", ImgUrl = "" };
                storkItmeServ.Create(storkItme);
                checkNr = StorkItmes().Count() + 1;
                Nr = context.StorkItme.Count();

                Assert.Equal(checkNr, Nr);
            }

        }


        private DataContext SetDataBaseUp(string databaseName)
        {

            var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: databaseName+"Database")
            .Options;

            DateTime dateTime = new DateTime().AddYears(1);

            using (var context = new DataContext(options))
            {
                context.UserGroup.AddRange(UserGroups());

                context.StorkItme.AddRange(StorkItmes());

                context.SaveChanges();
            }

            return new DataContext(options);
        }

        private List<UserGroup> UserGroups()
        {
            List<UserGroup> userGroups = new List<UserGroup>
            {
                new UserGroup() { Name = "test", Color = "#fff" }
            };

            return userGroups;
        }

        private List<StorkItme> StorkItmes()
        {
            DateTime dateTime = new DateTime().AddYears(1);

            List<StorkItme> storkItmess = new List<StorkItme>
            {
                new StorkItme() { UserGroupId = 1, Name = "den har id 1", Stork = 1, BestBy = dateTime, Description = "den har id 1", Type = "fefs", ImgUrl = "" },
                new StorkItme() { UserGroupId = 1, Name = "den har id 2", Stork = 1, BestBy = dateTime, Description = "den har id 2", Type = "fefs", ImgUrl = "" },
                new StorkItme() { UserGroupId = 1, Name = "den har id 3", Stork = 1, BestBy = dateTime, Description = "den har id 3", Type = "fefs", ImgUrl = "" }
            };

            return storkItmess;
        }

       

    }
}