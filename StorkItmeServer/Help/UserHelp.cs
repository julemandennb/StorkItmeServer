using System.Security.Claims;

namespace StorkItmeServer.Help
{
    public static class UserHelp
    {

        public static string Role(ClaimsPrincipal User)
        {
            var roles = User.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();
            if (roles.Count > 0)
            {
                return roles[0];
            }
            else
            {
                throw new Exception("User has no role claim.");
            }
        }

    }
}
