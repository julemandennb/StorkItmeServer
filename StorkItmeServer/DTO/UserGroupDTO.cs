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
    }
}
