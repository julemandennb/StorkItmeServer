using StorkItmeServer.Model;

namespace StorkItmeServer.Server.Interface
{
    public interface IUserGroupServ
    {
        public UserGroup? Get(int id);

        public IQueryable<UserGroup>? GetAll();

        public UserGroup? Create(UserGroup userGroup);

        public UserGroup? CreateWithoutSave(UserGroup userGroup);

        public bool Updata(UserGroup userGroup);

        public bool UpdateWithoutSave(UserGroup userGroup);

        public bool Delete(UserGroup userGroup);

        public bool DeleteWithoutSave(UserGroup userGroup);
    }
}
