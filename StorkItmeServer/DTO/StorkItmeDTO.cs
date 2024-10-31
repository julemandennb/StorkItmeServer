using StorkItmeServer.Model;

namespace StorkItmeServer.DTO
{
    public class StorkItmeDTO
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Type { get; set; }

        public DateTime BestBy { get; set; }

        public int Stork { get; set; }

        public string ImgUrl { get; set; }

        public  UserGroupDTO UserGroup { get; set; } = null!;

        public StorkItmeDTO() { }

        public StorkItmeDTO(StorkItme storkItme) {
        
            Id = storkItme.Id;
            Name = storkItme.Name;
            Description = storkItme.Description;
            Type = storkItme.Type;
            BestBy = storkItme.BestBy;
            Stork = storkItme.Stork;
            ImgUrl = storkItme.ImgUrl;
        }
    }
}
