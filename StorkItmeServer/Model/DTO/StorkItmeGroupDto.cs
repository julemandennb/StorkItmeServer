using StorkItmeServer.Model;

namespace StorkItmeServer.Model.DTO
{
    public class StorkItmeGroupDto
    {
        public Guid Uuid { get; set; }

        public string Name { get; set; }

        public string Description { get; set; } = string.Empty;

        public List<UserDTO> Users { get; set; }

        public List<StorkItmeDTO> StorkItmes { get; set; }

        public StorkItmeGroupDto() { }

        public StorkItmeGroupDto(StorkItmeGroup group)
        {
            Uuid = group.Uuid;
            Name = group.Name;
            Description = group.Description;
            Users = group.Users.Select(u => new UserDTO(u)).ToList();
            StorkItmes = group.StorkItmes.Select(s => new StorkItmeDTO(s)).ToList();
        }
    }
}
