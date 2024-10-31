using Microsoft.AspNetCore.Authorization;
using StorkItmeServer.Model;
using System.Security.Claims;

namespace StorkItmeServer.Handler
{
    public class RoleAuthorizationHandler : AuthorizationHandler<RoleRequirement>
    {
        // Define your role hierarchy and role system hav (from least to most privileged)
        public string[] roleHierarchy = ["Read","Member", "Manager", "Admin"];


        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            RoleRequirement requirement)
        {


            var UserRole = context.User.FindAll(ClaimTypes.Role).Select(role => role.Value).FirstOrDefault();


            if (UserRole is not null)
            {

                if(UserRole == requirement.RequiredRole || UserRole == roleHierarchy.Last())
                {
                    context.Succeed(requirement);
                    return Task.CompletedTask;
                }
                else if (RoleIndexCheck(UserRole, requirement.RequiredRole))
                {
                    context.Succeed(requirement);
                    return Task.CompletedTask;
                }

                // var roleHierarchy = new[] { "Member", "Manager", "Admin" };

                // Check if the user has any of the roles in the hierarchy
                //foreach (var role in roleHierarchy)
                //{
                //    if (context.User.IsInRole(role))
                //    {
                //        // If the user's role is at least the required role or higher in hierarchy
                //        if (this.roleHierarchy.Contains(requirement.RequiredRole) &&
                //            Array.IndexOf(this.roleHierarchy, role) >= Array.IndexOf(this.roleHierarchy, requirement.RequiredRole))
                //        {
                //            context.Succeed(requirement);
                //            return Task.CompletedTask;
                //        }
                //    }
                //}
            }

            return Task.CompletedTask;
        }

        private bool RoleIndexCheck(string userRole,string requirementRole)
        {
            int userRoleIndex = Array.IndexOf(this.roleHierarchy, userRole);

            int requirementRoleIndex = Array.IndexOf(this.roleHierarchy, requirementRole);

            return (userRoleIndex != -1 && requirementRoleIndex != -1) && userRoleIndex >= requirementRoleIndex;

        }


        public bool CheckUserRole(string role, string UserRole)
        {

            if (UserRole == role || UserRole == roleHierarchy.Last())
            {
                return true;
            }
            else if (RoleIndexCheck(UserRole, role))
            {
                return true;
            }

            return false;
        }


    }

    public class RoleRequirement : IAuthorizationRequirement
    {
        public string RequiredRole { get; }

        public RoleRequirement(string requiredRole)
        {
            RequiredRole = requiredRole;
        }
    }
}
