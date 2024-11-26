using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StorkItmeServer.Database;
using StorkItmeServer.Model;
using StorkItmeServer.Server;

namespace TestProject.Server
{
    public class UnitTestUserServ
    {
        private SetDataBaseUp _setDataBaseUp;

        public UnitTestUserServ()
        {
            _setDataBaseUp = new SetDataBaseUp("UserServ");
        }

        [Fact]
        public async void TestGet()
        {

            using (var context = _setDataBaseUp.Up("Get"))
            {
                UserServ userServ = MakeUserServ(context);

                 User user = context.Users.ToList()[1];
                Assert.NotNull(user);
                User use =  await userServ.Get(user.Id);
                Assert.NotNull(use);

                Assert.Equal(user, use);
            }

        }

        private UserServ MakeUserServ(DataContext context)
        {
            var userStore = new UserStore<User>(context);
            var userManager = new UserManager<User>(
                userStore,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null
            );

            var roleStore = new RoleStore<Role>(context);
            var roleManager = new RoleManager<Role>(
                roleStore,
                new IRoleValidator<Role>[0],
                new UpperInvariantLookupNormalizer(),
                new IdentityErrorDescriber(),
                new LoggerFactory().CreateLogger<RoleManager<Role>>()
            );

  
            UserServ userServ = new UserServ(null, context, userManager, roleManager,null,null,null, null);

            return userServ;

        }

    }
}
