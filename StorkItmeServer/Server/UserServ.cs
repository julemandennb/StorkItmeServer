using StorkItmeServer.Database;
using StorkItmeServer.Model;
using StorkItmeServer.Server.Interface;

namespace StorkItmeServer.Server
{
    public class UserServ: IUserServ
    {
        private readonly ILogger<UserServ> _logger;
        private readonly DataContext _context;

        public UserServ(ILogger<UserServ> logger, DataContext context)
        {
            _logger = logger;
            _context = context;

        }


        public User? Get(string id)
        {
            try
            {
                User user = _context.Users.FirstOrDefault(x => x.Id == id);

                return user == null ? null : user;
            }
            catch (Exception ex)
            {
                ErrorCatch(ex, "Get storkItme");
                return null;
            }
        }

        private void ErrorCatch(Exception ex, string funName)
        {
            _logger.LogError(ex, $"An error occurred while {funName}");
        }
    }
}
