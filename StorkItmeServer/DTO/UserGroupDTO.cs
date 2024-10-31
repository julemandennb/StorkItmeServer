using StorkItmeServer.Model;

namespace StorkItmeServer.DTO
{
    public class UserGroupDTO
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Color { get; set; }

        public List<UserDTO> Users { get; set; }

        public List<StorkItmeDTO> StorkItmes { get; set; }

        public UserGroupDTO() { }

        public UserGroupDTO(UserGroup userGroup) {
        
            Id = userGroup.Id;
            Name = userGroup.Name;
            Color = userGroup.Color;

        }
    }
}
