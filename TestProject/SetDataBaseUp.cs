using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StorkItmeServer.AuthorizationHandler;
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

        private RoleAuthorizationHandler roleAuthorizationHandler;

        internal SetDataBaseUp(string databaseClassName)
        {
            this.databaseClassName = databaseClassName;
            this.roleAuthorizationHandler = new RoleAuthorizationHandler();
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

                context.Users.AddRange(Users());

                context.Roles.AddRange(AddRole());

                context.SaveChanges();
            }

            return new DataContext(options);
        }

        private List<Role> AddRole()
        {
            return roleAuthorizationHandler.roleHierarchy.Select(x => new Role(x, x)).ToList();
        }
  
        internal List<User> Users()
        {
            List<User> users = new List<User>
            {
                new User(){ UserName ="User1",Email="user1@test.dk"},
                new User(){ UserName ="User2",Email="user2@test.dk"},
            };

            return users;
        }

        internal List<UserGroup> UserGroups()
        {
            List<UserGroup> userGroups = new List<UserGroup>
            {
                new UserGroup() { Name = "den har id 1", Color = "#fff" },
                new UserGroup() { Name = "den har id 2", Color = "#fff" },
                new UserGroup() { Name = "den har id 3", Color = "#fff" }
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
                new StorkItme() { UserGroupId = 1, Name = "den har id 3", Stork = 1, BestBy = dateTime, Description = "den har id 3", Type = "fefs", ImgUrl = "" },
                new StorkItme() { UserGroupId = 1, Name = "den har id 4", Stork = 1, BestBy = dateTime, Description = "den har id 4", Type = "fefs", ImgUrl = "" }
            };

            return storkItmess;
        }

    }
}
