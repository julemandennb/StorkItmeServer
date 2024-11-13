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

        public string Name { get; set; }

        public string Description { get; set; }

        public string Type { get; set; }

        public DateTime BestBy { get; set; }

        public int Stork { get; set; }

        public string ImgUrl { get; set; }

        public int UserGroupId { get; set; }

        public virtual UserGroup UserGroup { get; set; } = null!;
    }
}
