using StorkItmeServer.Model;

namespace StorkItmeServer.Model.DTO
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

        public string storeLocation { get; set; }

        public string ItemNumber { get; set; }

        public string EAN { get; set; }

        public  UserGroupDTO UserGroup { get; set; }

        public StorkItmeDTO() { }

        public StorkItmeDTO(StorkItme storkItme) {
        
            Id = storkItme.Id;
            Name = storkItme.Name;
            Description = storkItme.Description;
            Type = storkItme.Type;
            BestBy = storkItme.BestBy;
            Stork = storkItme.Stork;
            ImgUrl = storkItme.ImgUrl;
            storeLocation = storkItme.storeLocation;
            ItemNumber = storkItme.ItemNumber;
            EAN = storkItme.EAN;
        }
    }
}
