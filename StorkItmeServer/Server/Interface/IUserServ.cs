using Microsoft.AspNetCore.Identity;
using StorkItmeServer.Model;
using System.Security.Claims;

namespace StorkItmeServer.Server.Interface
{
    public interface IUserServ
    {
        public Task<User?> Get(string id);

        public Task<User?> GetByEmail(string email);

        public Task<User?> GetByClaimsPrincipal(ClaimsPrincipal userClaimsPrincipal);

        public Task<IdentityResult?> Create(User user, string password);

        public Task<bool> AddToRole(User user, string role);

        public Task<string> GenerateEmailConfirmationTokenAsync(User user);

        public Task<IdentityResult?> ConfirmEmailAsync(User user, string code);

        public Task<IdentityResult?> ChangeEmailAsync(User user, string? changedEmail, string code);

        public Task<IdentityResult?> SetUserNameAsync(User user, string? userName);

        public Task<bool> IsEmailConfirmedAsync(User user);

        public Task<string> GeneratePasswordResetTokenAsync(User user);

        public Task<IdentityResult?> ResetPasswordAsync(User user, string code, string NewPassword);

        public Task<IList<string>> GetRoles(User user);

        public IdentityError ErrorDescriberInvalidToken();

    }
}
