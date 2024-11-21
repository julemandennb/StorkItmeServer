using StorkItmeServer.Model;

namespace StorkItmeServer.Server.Interface
{
    public interface IUserServ
    {
        public User? Get(string id);
    }
}
