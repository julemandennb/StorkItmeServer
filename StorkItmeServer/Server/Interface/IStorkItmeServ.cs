using StorkItmeServer.Model;

namespace StorkItmeServer.Server.Interface
{
    public interface IStorkItmeServ
    {
        public StorkItme? Get(int id);

        public IQueryable<StorkItme>? GetAll();

        public StorkItme? Create(StorkItme storkItme);

        public StorkItme? CreateWithoutSave(StorkItme storkItme);

        public bool Updata(StorkItme storkItme);

        public bool UpdateWithoutSave(StorkItme storkItme);

        public bool Delete(StorkItme storkItme);

        public bool DeleteWithoutSave(StorkItme storkItme);

        public bool RemoveRange(ICollection<StorkItme> storkItmes);

        public bool RemoveRangeWithoutSave(ICollection<StorkItme> storkItmes);
    }
}
