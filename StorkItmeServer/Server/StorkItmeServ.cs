using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StorkItmeServer.Controllers;
using StorkItmeServer.Database;
using StorkItmeServer.Handler;
using StorkItmeServer.Model;
using StorkItmeServer.Model.DTO;
using StorkItmeServer.Server.Interface;

namespace StorkItmeServer.Server
{
    public class StorkItmeServ: IStorkItmeServ
    {
        private readonly ILogger<StorkItmeServ> _logger;
        private readonly DataContext _context;

        public StorkItmeServ(ILogger<StorkItmeServ> logger, DataContext context)
        {
            _logger = logger;
            _context = context;

        }

        public StorkItme? Get(int id)
        {
            try
            {
                StorkItme storkItme = _context.StorkItme.FirstOrDefault(x => x.Id == id);

                return storkItme == null ? null : storkItme;
            }
            catch (Exception ex)
            {
                ErrorCatch(ex, "Get storkItme");
                return null;
            }
        }

        public IQueryable<StorkItme>? GetAll()
        {
            try
            {
                IQueryable<StorkItme> StorkItmes = _context.StorkItme;

                 return StorkItmes;
            }
            catch (Exception ex)
            {
                ErrorCatch(ex, "GetAll storkItme");
                return null;
            }
        }

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
                ErrorCatch(ex, "Create storkItme");

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
                ErrorCatch(ex, "Create storkItme without save");
                return null;
            }
        }

        public bool Updata(StorkItme storkItme)
        {
            try
            {
                _context.StorkItme.Update(storkItme);
                _context.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                ErrorCatch(ex, "Updata storkItme");

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
                ErrorCatch(ex, "Update storkItme without save");
                return false;
            }
        }

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
                ErrorCatch(ex, "Delete storkItme");

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
                ErrorCatch(ex, "Delete storkItme without save");
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
                ErrorCatch(ex, "Delete storkItme");

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
                ErrorCatch(ex, "Remove range of storkItmes without save");
                return false;
            }
        }


        private void ErrorCatch(Exception ex,string funName)
        {
            if(_logger != null)
                _logger.LogError(ex, $"An error occurred while {funName}");
            else
                Console.WriteLine(ex);
        }

    }
}
