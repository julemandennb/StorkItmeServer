using Microsoft.Extensions.Hosting;

namespace StorkItmeServer.Model
{
    public class UserGroup
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Color { get; set; }

        public List<User> Users { get; } = [];

        public ICollection<StorkItme> StorkItmes { get; } = new List<StorkItme>();
    }
}
