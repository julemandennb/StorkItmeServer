using Microsoft.EntityFrameworkCore;
using StorkItmeServer.Database;
using StorkItmeServer.Model;
using StorkItmeServer.Server.Interface;

namespace StorkItmeServer.Server
{
    public class StorkItmeGroupServ : IGroupServ<StorkItmeGroup>
    {
        private readonly ILogger<StorkItmeGroupServ> _logger;
        private readonly DataContext _context;
        private readonly IStorkItmeServ _storkItmeServ;

        public StorkItmeGroupServ(ILogger<StorkItmeGroupServ> logger, DataContext context, IStorkItmeServ storkItmeServ)
        {
            _logger = logger;
            _context = context;
            _storkItmeServ = storkItmeServ;
        }

        // ------------------------
        // GET SINGLE
        // ------------------------
        public StorkItmeGroup? Get(int id)
        {
            try
            {
                StorkItmeGroup StorkItmeGroup = _context.StorkItmeGroup.FirstOrDefault(x => x.Id == id);

                return StorkItmeGroup == null ? null : StorkItmeGroup;
            }
            catch (Exception ex)
            {
                ErrorCatch(ex, "Get StorkItmeGroup");
                return null;
            }
        }

        public StorkItmeGroup? Get(string uuid)
        {
            try
            {
                if (!Guid.TryParse(uuid, out var guid))
                    return null;

                return _context.StorkItmeGroup
                    .FirstOrDefault(x => x.Uuid == guid);
            }
            catch (Exception ex)
            {
                ErrorCatch(ex, "Get StorkItmeGroup");
                return null;
            }
        }

        // ------------------------
        // LISTS (READ ONLY)
        // ------------------------
        public async Task<List<StorkItmeGroup>> GetAll(string userId = "", bool GetAll = false, bool includeStorkItmes = false, bool includeUsers = false)
        {
            try
            {
                IQueryable<StorkItmeGroup> query = _context.StorkItmeGroup;

                if (!GetAll)
                {
                    query = query.Where(x => x.Users.Any(u => u.Id == userId));
                }

                if (includeStorkItmes)
                {
                    query = query.Include(x => x.StorkItmes);
                }

                if (includeUsers)
                {
                    query = query.Include(x => x.Users);
                }

                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                ErrorCatch(ex, "GetAll StorkItmeGroup");
                return new List<StorkItmeGroup>();
            }
        }

        // ------------------------
        // CREATE
        // ------------------------
        public StorkItmeGroup? Create(StorkItmeGroup StorkItmeGroup)
        {
            try
            {
                _context.StorkItmeGroup.Add(StorkItmeGroup);
                _context.SaveChanges();

                return StorkItmeGroup;
            }
            catch (Exception ex)
            {
                ErrorCatch(ex, "Create StorkItmeGroup");

                return null;
            }
        }

        public StorkItmeGroup? CreateWithoutSave(StorkItmeGroup StorkItmeGroup)
        {
            try
            {
                _context.StorkItmeGroup.Add(StorkItmeGroup);

                return StorkItmeGroup;
            }
            catch (Exception ex)
            {
                ErrorCatch(ex, "Create StorkItmeGroup");

                return null;
            }
        }

        // ------------------------
        // UPDATE
        // ------------------------
        public bool Update(StorkItmeGroup StorkItmeGroup)
        {
            try
            {
                _context.StorkItmeGroup.Update(StorkItmeGroup);
                _context.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                ErrorCatch(ex, "Updata StorkItmeGroup");

                return false;
            }
        }

        public bool UpdateWithoutSave(StorkItmeGroup StorkItmeGroup)
        {
            try
            {
                _context.StorkItmeGroup.Update(StorkItmeGroup);
                return true;
            }
            catch (Exception ex)
            {
                ErrorCatch(ex, "Update StorkItmeGroup without save");
                return false;
            }
        }

        // ------------------------
        // DELETE
        // ------------------------
        public bool Delete(StorkItmeGroup StorkItmeGroup)
        {
            try
            {
                StorkItmeGroup.Users.Clear();

                _context.StorkItme.RemoveRange(StorkItmeGroup.StorkItmes);

                _context.StorkItmeGroup.Remove(StorkItmeGroup);

                _context.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                ErrorCatch(ex, "Delete StorkItmeGroup");
                return false;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var storkItmeGroup = await _context.StorkItmeGroup
                .Include(x => x.Users)
                .Include(x => x.StorkItmes)
                .FirstOrDefaultAsync(x => x.Id == id);

                if (storkItmeGroup == null)
                    return false;

                storkItmeGroup.Users.Clear();
                _context.StorkItme.RemoveRange(storkItmeGroup.StorkItmes);
                _context.StorkItmeGroup.Remove(storkItmeGroup);

                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                ErrorCatch(ex, "DeleteAsync StorkItmeGroup");
                return false;
            }

        }

        public bool DeleteWithoutSave(StorkItmeGroup StorkItmeGroup)
        {
            try
            {
                _context.StorkItmeGroup.Remove(StorkItmeGroup);
                return true;
            }
            catch (Exception ex)
            {
                ErrorCatch(ex, "Delete StorkItmeGroup without save");
                return false;
            }
        }

        // ------------------------
        // HELPERS
        // ------------------------
        private void ErrorCatch(Exception ex, string funName)
        {
            if (_logger != null)
                _logger.LogError(ex, $"An error occurred while {funName}");
            else
                Console.WriteLine(ex);
        }
    }
}
