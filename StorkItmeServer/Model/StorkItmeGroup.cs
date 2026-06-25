using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata;

namespace StorkItmeServer.Model
{
    public class StorkItmeGroup
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public required string Name { get; set; }

        public required string Description { get; set; }

        public virtual ICollection<StorkItme> StorkItmes { get; set; } = new List<StorkItme>();

        public virtual ICollection<User> Users { get; set; } = [];


    }
}
