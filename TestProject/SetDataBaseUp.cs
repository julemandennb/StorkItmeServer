using Microsoft.EntityFrameworkCore;
using StorkItmeServer.AuthorizationHandler;
using StorkItmeServer.Database;
using StorkItmeServer.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TestProject
{
    internal class SetDataBaseUp
    {
        private readonly string databaseClassName;
        private readonly RoleAuthorizationHandler roleAuthorizationHandler;

        internal SetDataBaseUp(string databaseClassName)
        {
            this.databaseClassName = databaseClassName;
            this.roleAuthorizationHandler = new RoleAuthorizationHandler();
        }

        internal DataContext Up(string databaseName)
        {
            var options = new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase(databaseName: $"{databaseClassName}-{databaseName}-{Guid.NewGuid()}")
                .Options;

            var context = new DataContext(options);

            // Seed data
            context.UserGroup.AddRange(UserGroups());
            context.SaveChanges();

            context.StorkItmeGroup.AddRange(StorkItmeGroups());
            context.SaveChanges();

            context.StorkItme.AddRange(StorkItmes());
            context.Users.AddRange(Users());
            context.Roles.AddRange(AddRole());

            context.SaveChanges();

            return context;
        }

        private List<Role> AddRole()
        {
            return roleAuthorizationHandler.roleHierarchy
                .Select(x => new Role(x, x))
                .ToList();
        }

        internal List<User> Users()
        {
            return new List<User>
            {
                new User { UserName = "User1", Email = "user1@test.dk" },
                new User { UserName = "User2", Email = "user2@test.dk" }
            };
        }

        internal List<UserGroup> UserGroups()
        {
            return new List<UserGroup>
            {
                new UserGroup { Name = "den har id 1", Color = "#fff" },
                new UserGroup { Name = "den har id 2", Color = "#fff" },
                new UserGroup { Name = "den har id 3", Color = "#fff" }
            };
        }

        internal List<StorkItmeGroup> StorkItmeGroups()
        {
            return new List<StorkItmeGroup>
            {
                new StorkItmeGroup { Name = "den har id 1" , Description = "test 1" },
                new StorkItmeGroup { Name = "den har id 2" , Description = "test 2" },
                new StorkItmeGroup { Name = "den har id 3" , Description = "test 3" }
            };
        }

        internal List<StorkItme> StorkItmes()
        {
            DateTime now = DateTime.UtcNow;

            return new List<StorkItme>
            {
                new StorkItme
                {
                    UserGroupId = 1,
                    Name = "den har id 1",
                    Stork = 1,
                    BestBy = now.AddYears(1),
                    Description = "den har id 1",
                    Type = "fefs",
                    ImgUrl = "",
                    StorkItmeGroupId = 1,
                },
                new StorkItme
                {
                    UserGroupId = 1,
                    Name = "den har id 2",
                    Stork = 1,
                    BestBy = now.AddYears(1),
                    Description = "den har id 2",
                    Type = "fefs",
                    ImgUrl = "",
                    StorkItmeGroupId = 1
                },
                new StorkItme
                {
                    UserGroupId = 1,
                    Name = "den har id 3",
                    Stork = 1,
                    BestBy = now.AddDays(-1),
                    Description = "den har id 3",
                    Type = "fefs",
                    ImgUrl = "",
                    StorkItmeGroupId = 1
                },
                new StorkItme
                {
                    UserGroupId = 2,
                    Name = "den har id 4",
                    Stork = 1,
                    BestBy = now.AddDays(1),
                    Description = "den har id 4",
                    Type = "fefs",
                    ImgUrl = "",
                    StorkItmeGroupId = 2,

                }
            };
        }
    }
}