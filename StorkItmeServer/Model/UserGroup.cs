using Microsoft.Extensions.Hosting;

namespace StorkItmeServer.Model
{
    public class UserGroup
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Color { get; set; }

        public virtual ICollection<User> Users { get; set; } = [];

        public virtual ICollection<StorkItme> StorkItmes { get; set; } = new List<StorkItme>();
    }
}
