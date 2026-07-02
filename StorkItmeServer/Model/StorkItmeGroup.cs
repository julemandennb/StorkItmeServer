using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace StorkItmeServer.Model
{
    /// <summary>
    /// Represents a storage group (e.g., refrigerator section or category)
    /// </summary>
    public class StorkItmeGroup
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public Guid Uuid { get; private set; } = Guid.NewGuid();

        public required string Name { get; set; }

        public required string Description { get; set; }

        public virtual ICollection<StorkItme> StorkItmes { get; set; } = new List<StorkItme>();

        public virtual ICollection<User> Users { get; set; } = new List<User>();


    }
}
