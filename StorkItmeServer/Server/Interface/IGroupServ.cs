using StorkItmeServer.Model;

namespace StorkItmeServer.Server.Interface
{
    public interface IGroupServ<T>
    {
       public T? Get(int id);

        public T? Get(string uuid);

        public Task<List<T>>? GetAll(string userId = "", bool GetAll = false, bool includeStorkItmes = false, bool includeUsers = false);

        public T? Create(T item);

        public T? CreateWithoutSave(T item);

        public bool Update(T item);

        public bool UpdateWithoutSave(T item);

        public bool Delete(T item);

        public Task<bool> DeleteAsync(int id);

        public bool DeleteWithoutSave(T item);
    }
}
