using StorkItmeServer.Model;

namespace StorkItmeServer.Server.Interface
{
    public interface IStorkItmeServ
    {
        // --------------------
        // SINGLE GET
        // --------------------

        StorkItme? Get(int id);

        StorkItme? GetFromItemNumber(string itemNumber);

        StorkItme? GetFromEAN(string ean);

        // --------------------
        // LIST GETTERS
        // --------------------

        List<StorkItme> GetAll();

        List<StorkItme> GetAll7DaysBeforeBestBy();

        List<StorkItme> GetAllAfterBestBy();

        // --------------------
        // CREATE
        // --------------------

        StorkItme? Create(StorkItme storkItme);

        StorkItme? CreateWithoutSave(StorkItme storkItme);

        // --------------------
        // UPDATE
        // --------------------

        bool Update(StorkItme storkItme);

        bool UpdateWithoutSave(StorkItme storkItme);

        // --------------------
        // DELETE
        // --------------------

        bool Delete(StorkItme storkItme);

        bool DeleteWithoutSave(StorkItme storkItme);

        bool RemoveRange(ICollection<StorkItme> storkItmes);

        bool RemoveRangeWithoutSave(ICollection<StorkItme> storkItmes);
    }
}