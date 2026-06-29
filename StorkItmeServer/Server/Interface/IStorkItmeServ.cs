using StorkItmeServer.Model;

namespace StorkItmeServer.Server.Interface
{
    public interface IStorkItmeServ
    {
        // --------------------
        // SINGLE GET
        // --------------------

        Task<StorkItme?> GetAsync(int id);

        Task<StorkItme?> GetAsync(string uuid);

        Task<StorkItme?> GetFromItemNumberAsync(string itemNumber);

        Task<StorkItme?> GetFromEANAsync(string ean);

        // --------------------
        // LIST GETTERS
        // --------------------

        Task<List<StorkItme>> GetAllAsync();

        Task<List<StorkItme>> GetAll7DaysBeforeBestByAsync();

        Task<List<StorkItme>> GetAllAfterBestByAsync();

        // --------------------
        // CREATE
        // --------------------

        Task<StorkItme?> CreateAsync(StorkItme storkItme);

        // --------------------
        // UPDATE
        // --------------------

        Task<bool> UpdateAsync(StorkItme storkItme);

        // --------------------
        // DELETE
        // --------------------

        Task<bool> DeleteAsync(int id);

        Task<bool> DeleteAsync(string uuid);

        Task<bool> RemoveRangeAsync(ICollection<StorkItme> storkItmes);
    }
}