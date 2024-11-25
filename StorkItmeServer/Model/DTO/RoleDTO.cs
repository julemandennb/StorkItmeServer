namespace StorkItmeServer.Model.DTO
{
    public class RoleDTO
    {

        public string DisplayName { get; set; } = "";
        public string Name { get; set; }

        public RoleDTO() { 
        }

        public RoleDTO(Role role)
        {
            DisplayName = role.DisplayName;
            Name = role.Name;
        }
    }
}
