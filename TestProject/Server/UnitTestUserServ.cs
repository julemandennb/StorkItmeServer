using Microsoft.EntityFrameworkCore;
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
        public void TestGet()
        {

            using (var context = _setDataBaseUp.Up("Get"))
            {
                UserServ userServ = new UserServ(null, context);

                User user = context.Users.ToList()[1];
                Assert.NotNull(user);
                User use = userServ.Get(user.Id);
                Assert.NotNull(use);

                Assert.Equal(user, use);
            }

        }

    }
}
