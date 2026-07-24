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

        Task<StorkItme?> GetTrackedAsync(string uuid);

        // --------------------
        // LIST GETTERS
        // --------------------

        Task<List<StorkItme>> GetAllAsync(HashSet<int>? groupIds = null, HashSet<int>? storkItmeGroupIds = null);

        Task<List<StorkItme>> GetAll7DaysBeforeBestByAsync(HashSet<int>? groupIds = null, HashSet<int>? storkItmeGroupIds = null);

        Task<List<StorkItme>> GetAllNotExpiredAsync(HashSet<int>? groupIds = null, HashSet<int>? storkItmeGroupIds = null);

        // --------------------
        // CREATE
        // --------------------

        Task<StorkItme?> CreateAsync(StorkItme storkItme);

        // --------------------
        // UPDATE
        // --------------------

        // Returns a tuple: Success flag and optional error message when Success is false
        Task<(bool Success, string? ErrorMessage)> UpdateAsync();

        // --------------------
        // DELETE
        // --------------------

        Task<bool> DeleteAsync(int id);

        Task<bool> DeleteAsync(string uuid);

        Task<bool> RemoveRangeAsync(ICollection<StorkItme> storkItmes);
    }
}