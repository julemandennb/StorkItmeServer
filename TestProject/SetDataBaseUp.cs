using Microsoft.EntityFrameworkCore;
using StorkItmeServer.Database;
using StorkItmeServer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject
{
    internal class SetDataBaseUp
    {

        private string databaseClassName = "";

        internal SetDataBaseUp(string databaseClassName)
        {
            this.databaseClassName = databaseClassName;
        }



        internal DataContext Up(string databaseName)
        {

            var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: $"{this.databaseClassName}{databaseName}Database")
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

        internal List<UserGroup> UserGroups()
        {
            List<UserGroup> userGroups = new List<UserGroup>
            {
                new UserGroup() { Name = "test", Color = "#fff" }
            };

            return userGroups;
        }

        internal List<StorkItme> StorkItmes()
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
