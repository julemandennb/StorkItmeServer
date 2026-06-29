using StorkItmeServer.Model;

namespace StorkItmeServer.Model.DTO
{
    public class UserGroupDTO
    {
        public Guid Uuid { get; set; }

        public string Name { get; set; }

        public string Color { get; set; }

        public List<UserDTO> Users { get; set; }

        public List<StorkItmeDTO> StorkItmes { get; set; }

        public UserGroupDTO() { }

        public UserGroupDTO(UserGroup userGroup) {

            Uuid = userGroup.Uuid;
            Name = userGroup.Name;
            Color = userGroup.Color;

        }
    }
}
