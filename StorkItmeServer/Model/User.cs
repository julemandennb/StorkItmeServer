using Microsoft.AspNetCore.Identity;

namespace StorkItmeServer.Model
{
    public class User : IdentityUser
    {
        public List<UserGroup> UserGroups { get; } = [];
    }
}
