using Microsoft.EntityFrameworkCore;
using StorkItmeServer.Database;
using StorkItmeServer.Model;
using StorkItmeServer.Server.Interface;

namespace StorkItmeServer.Server
{
    public class StorkItmeServ : IStorkItmeServ
    {
        private readonly ILogger<StorkItmeServ> _logger;
        private readonly DataContext _context;

        public StorkItmeServ(ILogger<StorkItmeServ> logger, DataContext context)
        {
            _logger = logger;
            _context = context;
        }

        // ------------------------
        // BASE QUERY
        // ------------------------
        private IQueryable<StorkItme> BaseQuery(bool tracking = false)
        {
            var query = _context.StorkItme
                .Include(x => x.UserGroup);

            return tracking ? query : query.AsNoTracking();
        }

        private DateTime UtcNow => DateTime.UtcNow;

        // ------------------------
        // SINGLE GET
        // ------------------------

        public async Task<StorkItme?> GetAsync(int id)
        {
            return await BaseQuery()
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<StorkItme?> GetAsync(string uuid)
        {
            if (!Guid.TryParse(uuid, out var guid))
                return null;

            return await BaseQuery()
                .FirstOrDefaultAsync(x => x.Uuid == guid);
        }

        public async Task<StorkItme?> GetFromItemNumberAsync(string itemNumber)
        {
            return await BaseQuery()
                .FirstOrDefaultAsync(x => x.ItemNumber == itemNumber);
        }

        public async Task<StorkItme?> GetFromEANAsync(string ean)
        {
            return await BaseQuery()
                .FirstOrDefaultAsync(x => x.EAN == ean);
        }

        // ------------------------
        // LIST GETTERS
        // ------------------------

        public async Task<List<StorkItme>> GetAllAsync(
            HashSet<int>? groupIds = null,
            HashSet<int>? storkItmeGroupIds = null)
        {
            var query = ApplyGroupFilter(BaseQuery(), groupIds, storkItmeGroupIds);

            return await query
                .OrderBy(x => x.Id)
                .ToListAsync();
        }

        public async Task<List<StorkItme>> GetAll7DaysBeforeBestByAsync(
            HashSet<int>? groupIds = null,
            HashSet<int>? storkItmeGroupIds = null)
        {
            var now = UtcNow;
            var inSevenDays = now.AddDays(7);

            var query = ApplyGroupFilter(BaseQuery(), groupIds, storkItmeGroupIds);

            return await query
                .Where(x => x.BestBy >= now && x.BestBy <= inSevenDays)
                .OrderBy(x => x.Id)
                .ToListAsync();
        }

        public async Task<List<StorkItme>> GetAllNotExpiredAsync(
          HashSet<int>? groupIds = null,
          HashSet<int>? storkItmeGroupIds = null)
        {
            var now = UtcNow;

            var query = ApplyGroupFilter(BaseQuery(), groupIds, storkItmeGroupIds);

            return await query
                .Where(x => x.BestBy >= now).OrderBy(x => x.Id)
                .ToListAsync();
        }

        // ------------------------
        // CREATE
        // ------------------------

        public async Task<StorkItme?> CreateAsync(StorkItme storkItme)
        {
            try
            {
                _context.StorkItme.Add(storkItme);
                await _context.SaveChangesAsync();
                return storkItme;
            }
            catch (Exception ex)
            {
                LogError(ex, "Create storkItme");
                return null;
            }
        }

        // ------------------------
        // UPDATE
        // ------------------------

        public async Task<bool> UpdateAsync(StorkItme storkItme)
        {
            try
            {
                var existing = await _context.StorkItme
                    .FirstOrDefaultAsync(x => x.Id == storkItme.Id);

                if (existing == null)
                    return false;

                _context.Entry(existing).CurrentValues.SetValues(storkItme);

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                LogError(ex, "Update storkItme");
                return false;
            }
        }

        // ------------------------
        // DELETE
        // ------------------------

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var entity = await _context.StorkItme
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (entity == null)
                    return false;

                _context.StorkItme.Remove(entity);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                LogError(ex, "Delete storkItme");
                return false;
            }
        }

        public async Task<bool> DeleteAsync(string uuid)
        {
            try
            {

                if (!Guid.TryParse(uuid, out var guid))
                    return false;

                var entity = await _context.StorkItme
                     .FirstOrDefaultAsync(x => x.Uuid == guid);

                if (entity == null)
                    return false;

                _context.StorkItme.Remove(entity);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                LogError(ex, "Delete storkItme");
                return false;
            }
        }

        public async Task<bool> RemoveRangeAsync(ICollection<StorkItme> storkItmes)
        {
            try
            {
                _context.StorkItme.RemoveRange(storkItmes);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                LogError(ex, "Remove range storkItme");
                return false;
            }
        }

        // ------------------------
        // LOGGING
        // ------------------------

        private void LogError(Exception ex, string funName)
        {
            _logger.LogError(ex, "An error occurred while {Function}", funName);
        }

        // ------------------------
        // HELPERS
        // ------------------------

        private static IQueryable<StorkItme> ApplyGroupFilter(
           IQueryable<StorkItme> query,
           HashSet<int>? groupIds,
           HashSet<int>? storkItmeGroupIds)
        {
            if (groupIds == null && storkItmeGroupIds == null)
                return query;

            return query.Where(x =>
                (groupIds != null &&
                 x.UserGroupId.HasValue &&
                 groupIds.Contains(x.UserGroupId.Value))
                ||
                (storkItmeGroupIds != null &&
                 x.StorkItmeGroupId.HasValue &&
                 storkItmeGroupIds.Contains(x.StorkItmeGroupId.Value))
            );
        }
    }
}