using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection.Metadata;

namespace StorkItmeServer.Model
{
    public class StorkItme
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public Guid Uuid { get; private set; } = Guid.NewGuid();

        public required string Name { get; set; }

        public required string Description { get; set; }

        public required string Type { get; set; }

        public DateTime BestBy { get; set; }

        public int Stork { get; set; }

        public string ?ImgUrl { get; set; }

        public string ?StoreLocation { get; set; }

        public string ?ItemNumber { get; set; }

        public string ?EAN { get; set; }

        public int ?UserGroupId { get; set; }

        public int? StorkItmeGroupId { get; set; }

        public virtual UserGroup UserGroup { get; set; } = null!;

        public virtual StorkItmeGroup StorkItmeGroup { get; set;} = null!;
    }
}
