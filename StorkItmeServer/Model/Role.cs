using Microsoft.AspNetCore.Identity;

namespace StorkItmeServer.Model
{
    public class Role : IdentityRole
    {
        public string DisplayName { get; set; } = "";


        public Role(string roleName, string DisplayName) : base(roleName)
        {
            this.DisplayName = DisplayName;
        }

        public Role()
        { }

       
    }
}
