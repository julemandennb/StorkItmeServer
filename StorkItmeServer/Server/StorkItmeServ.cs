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
        // GET SINGLE ITEMS
        // ------------------------

        public StorkItme? Get(int id)
        {
            try
            {
                return _context.StorkItme.FirstOrDefault(x => x.Id == id);
            }
            catch (Exception ex)
            {
                LogError(ex, "Get storkItme");
                return null;
            }
        }

        public StorkItme? GetFromItemNumber(string itemNumber)
        {
            try
            {
                return _context.StorkItme.FirstOrDefault(x => x.ItemNumber == itemNumber);
            }
            catch (Exception ex)
            {
                LogError(ex, "Get storkItme from ItemNumber");
                return null;
            }
        }

        public StorkItme? GetFromEAN(string ean)
        {
            try
            {
                return _context.StorkItme.FirstOrDefault(x => x.EAN == ean);
            }
            catch (Exception ex)
            {
                LogError(ex, "Get storkItme from EAN");
                return null;
            }
        }

        // ------------------------
        // GET LISTS
        // ------------------------

        public List<StorkItme> GetAll()
        {
            try
            {
                return _context.StorkItme
                    .Include(x => x.UserGroup)
                    .OrderBy(x => x.Id)
                    .ToList();
            }
            catch (Exception ex)
            {
                LogError(ex, "GetAll storkItme");
                return new List<StorkItme>();
            }
        }

        public List<StorkItme> GetAll7DaysBeforeBestBy()
        {
            try
            {
                var now = DateTime.UtcNow;
                var inSevenDays = now.AddDays(7);

                return _context.StorkItme
                    .Include(x => x.UserGroup)
                    .Where(x => x.BestBy >= now && x.BestBy <= inSevenDays)
                    .ToList();
            }
            catch (Exception ex)
            {
                LogError(ex, "GetAll 7 Days Before BestBy");
                return new List<StorkItme>();
            }
        }

        public List<StorkItme> GetAllAfterBestBy()
        {
            try
            {
                var now = DateTime.UtcNow;

                return _context.StorkItme
                    .Include(x => x.UserGroup)
                    .Where(x => x.BestBy <= now)
                    .ToList();
            }
            catch (Exception ex)
            {
                LogError(ex, "GetAll after BestBy");
                return new List<StorkItme>();
            }
        }

        // ------------------------
        // CREATE
        // ------------------------

        public StorkItme? Create(StorkItme storkItme)
        {
            try
            {
                _context.StorkItme.Add(storkItme);
                _context.SaveChanges();
                return storkItme;
            }
            catch (Exception ex)
            {
                LogError(ex, "Create storkItme");
                return null;
            }
        }

        public StorkItme? CreateWithoutSave(StorkItme storkItme)
        {
            try
            {
                _context.StorkItme.Add(storkItme);
                return storkItme;
            }
            catch (Exception ex)
            {
                LogError(ex, "Create storkItme without save");
                return null;
            }
        }

        // ------------------------
        // UPDATE
        // ------------------------

        public bool Update(StorkItme storkItme)
        {
            try
            {
                _context.StorkItme.Update(storkItme);
                _context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                LogError(ex, "Update storkItme");
                return false;
            }
        }

        public bool UpdateWithoutSave(StorkItme storkItme)
        {
            try
            {
                _context.StorkItme.Update(storkItme);
                return true;
            }
            catch (Exception ex)
            {
                LogError(ex, "Update storkItme without save");
                return false;
            }
        }

        // ------------------------
        // DELETE
        // ------------------------

        public bool Delete(StorkItme storkItme)
        {
            try
            {
                _context.StorkItme.Remove(storkItme);
                _context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                LogError(ex, "Delete storkItme");
                return false;
            }
        }

        public bool DeleteWithoutSave(StorkItme storkItme)
        {
            try
            {
                _context.StorkItme.Remove(storkItme);
                return true;
            }
            catch (Exception ex)
            {
                LogError(ex, "Delete storkItme without save");
                return false;
            }
        }

        public bool RemoveRange(ICollection<StorkItme> storkItmes)
        {
            try
            {
                _context.StorkItme.RemoveRange(storkItmes);
                _context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                LogError(ex, "Remove range storkItme");
                return false;
            }
        }

        public bool RemoveRangeWithoutSave(ICollection<StorkItme> storkItmes)
        {
            try
            {
                _context.StorkItme.RemoveRange(storkItmes);
                return true;
            }
            catch (Exception ex)
            {
                LogError(ex, "Remove range storkItme without save");
                return false;
            }
        }

        // ------------------------
        // LOGGING
        // ------------------------

        private void LogError(Exception ex, string operation)
        {
            _logger.LogError(ex, "An error occurred while {Operation}", operation);
        }
    }
}