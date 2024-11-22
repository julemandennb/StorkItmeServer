using StorkItmeServer.Database;
using StorkItmeServer.Model;
using StorkItmeServer.Server.Interface;

namespace StorkItmeServer.Server
{
    public class UserGroupServ: IUserGroupServ
    {
        private readonly ILogger<UserGroupServ> _logger;
        private readonly DataContext _context;
        private readonly IStorkItmeServ _storkItmeServ;

        public UserGroupServ(ILogger<UserGroupServ> logger, DataContext context, IStorkItmeServ storkItmeServ) { 
            _logger = logger;
            _context = context;
            _storkItmeServ = storkItmeServ;
        }

        public UserGroup? Get(int id)
        {
            try
            {
                UserGroup userGroup = _context.UserGroup.FirstOrDefault(x => x.Id == id);

                return userGroup == null ? null : userGroup;
            }
            catch (Exception ex)
            {
                ErrorCatch(ex, "Get UserGroup");
                return null;
            }
        }

        public IQueryable<UserGroup>? GetAll()
        {
            try
            {
                IQueryable<UserGroup> userGroups = _context.UserGroup;

                return userGroups;
            }
            catch (Exception ex)
            {
                ErrorCatch(ex, "GetAll userGroups");
                return null;
            }
        }

        public UserGroup? Create(UserGroup userGroup)
        {
            try
            {
                _context.UserGroup.Add(userGroup);
                _context.SaveChanges();

                return userGroup;
            }
            catch (Exception ex)
            {
                ErrorCatch(ex, "Create userGroup");

                return null;
            }
        }

        public UserGroup? CreateWithoutSave(UserGroup userGroup)
        {
            try
            {
                _context.UserGroup.Add(userGroup);

                return userGroup;
            }
            catch (Exception ex)
            {
                ErrorCatch(ex, "Create userGroup");

                return null;
            }
        }

        public bool Updata(UserGroup userGroup)
        {
            try
            {
                _context.UserGroup.Update(userGroup);
                _context.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                ErrorCatch(ex, "Updata userGroup");

                return false;
            }
        }

        public bool UpdateWithoutSave(UserGroup userGroup)
        {
            try
            {
                _context.UserGroup.Update(userGroup);
                return true;
            }
            catch (Exception ex)
            {
                ErrorCatch(ex, "Update userGroup without save");
                return false;
            }
        }

        public bool Delete(UserGroup userGroup)
        {
            try
            {
                userGroup.Users.Clear();

                ICollection<StorkItme> storkItmes = userGroup.StorkItmes;

                _storkItmeServ.RemoveRangeWithoutSave(storkItmes);

                _context.UserGroup.Remove(userGroup);

                _context.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                ErrorCatch(ex, "Delete userGroup");

                return false;
            }
        }

        public bool DeleteWithoutSave(UserGroup userGroup)
        {
            try
            {
                _context.UserGroup.Remove(userGroup);
                return true;
            }
            catch (Exception ex)
            {
                ErrorCatch(ex, "Delete userGroup without save");
                return false;
            }
        }


        private void ErrorCatch(Exception ex, string funName)
        {
            if (_logger != null)
                _logger.LogError(ex, $"An error occurred while {funName}");
            else
                Console.WriteLine(ex);
        }

    }
}
