namespace StorkItmeServer.Model
{
    /// <summary>
    /// Represents a user group (e.g., Dairy Technician) that defines
    /// which users belong to the group and what items they work on.
    /// </summary>
    public class UserGroup
    {
        public int Id { get; set; }

        public Guid Uuid { get; private set; } = Guid.NewGuid();

        public string Name { get; set; }

        public string Color { get; set; }

        public virtual ICollection<User> Users { get; set; } = new List<User>();

        public virtual ICollection<StorkItme> StorkItmes { get; set; } = new List<StorkItme>();
    }
}
