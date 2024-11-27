using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StorkItmeServer.Controllers;
using StorkItmeServer.Database;
using StorkItmeServer.AuthorizationHandler;
using StorkItmeServer.Model;
using StorkItmeServer.Server.Interface;
using System;
using System.Data;
using System.Security.Claims;

namespace StorkItmeServer.Server
{
    public class UserServ: IUserServ
    {
        private readonly ILogger<UserServ> _logger;
        private readonly DataContext _context;
        private readonly UserManager<User> _userManager;

        private readonly RoleManager<Role> _roleManager;
        private readonly IEmailSender<User> _emailSender;
        private readonly SignInManager<User> _signInManager;
        private readonly TimeProvider _timeProvider;
        private readonly IOptionsMonitor<BearerTokenOptions> _bearerTokenOptions;

        public UserServ(ILogger<UserServ> logger, DataContext context, UserManager<User> userManager, RoleManager<Role> roleManager,
            IEmailSender<User> emailSender, SignInManager<User> signInManager,
            TimeProvider timeProvider, IOptionsMonitor<BearerTokenOptions> optionsMonitor)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;

            _roleManager = roleManager;
            _emailSender = emailSender;
            _signInManager = signInManager;
            _timeProvider = timeProvider;
            _bearerTokenOptions = optionsMonitor;

        }


        public async Task<User?> Get(string id)
        {
            try
            {
                
                User user = await _userManager.FindByIdAsync(id);

                return user == null ? null : user;
            }
            catch (Exception ex)
            {
                ErrorCatch(ex, "Get User by id");
                return null;
            }
        }

        public async Task<User?> GetByEmail(string email)
        {
            try
            {

                User user = await _userManager.FindByEmailAsync(email);

                return user == null ? null : user;
            }
            catch (Exception ex)
            {
                ErrorCatch(ex, "Get User by Email");
                return null;
            }
        }

        public async Task<User?> GetByClaimsPrincipal(ClaimsPrincipal userClaimsPrincipal)
        {
            try
            {
                User user = await _userManager.GetUserAsync(userClaimsPrincipal);

                return user == null ? null : user;
            }
            catch (Exception ex)
            {
                ErrorCatch(ex, "Get User by ClaimsPrincipal");
                return null;
            }
        }

        public async Task<IdentityResult?> Create(User user,string password)
        {
            try
            {
                IdentityResult identityResult = await _userManager.CreateAsync(user, password);

                return identityResult;
            }
            catch (Exception ex)
            {
                ErrorCatch(ex, "Create User");
                return IdentityResult.Failed(new IdentityError { Description = "An error occurred while creating the user." });
            }
        }

        public async Task<bool> AddToRole(User user,string role)
        {
            try
            {
                IdentityResult identityResult = await _userManager.AddToRoleAsync(user, role);

                return identityResult.Succeeded;
            }
            catch (Exception ex)
            {
                ErrorCatch(ex, "add role to User");
                return false;
            }
        }

        public async Task<string> GenerateEmailConfirmationTokenAsync(User user)
        {
            try
            {
               string EmailConfirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                return EmailConfirmationToken;
            }
            catch (Exception ex)
            {
                ErrorCatch(ex, "Generate Email Confirmation Token");
                return "";
            }
        }

        public async Task<IdentityResult?> ConfirmEmailAsync(User user, string code)
        {
            try
            {
                IdentityResult identityResult = await _userManager.ConfirmEmailAsync(user, code);

                return identityResult;
            }
            catch (Exception ex)
            {
                ErrorCatch(ex, "Confirm Email");
                return IdentityResult.Failed(new IdentityError { Description = "An error occurred while Confirm Email." });
            }
        }

        public async Task<IdentityResult?> ChangeEmailAsync(User user,string? changedEmail, string code)
        {
            try
            {
                IdentityResult identityResult = await _userManager.ChangeEmailAsync(user, changedEmail, code);

                return identityResult;
            }
            catch (Exception ex)
            {
                ErrorCatch(ex, "Change Email");
                return IdentityResult.Failed(new IdentityError { Description = "An error occurred while Change Email" });
            }
        }

        public async Task<IdentityResult?> SetUserNameAsync(User user, string? userName)
        {
            try
            {
                IdentityResult identityResult = await _userManager.SetUserNameAsync(user, userName);

                return identityResult;
            }
            catch (Exception ex)
            {
                ErrorCatch(ex, "Set User Name");
                return IdentityResult.Failed(new IdentityError { Description = "An error occurred while Set User Name" });
            }
        }

        public async Task<bool> IsEmailConfirmedAsync(User user)
        {
            try
            {
               
                return await _userManager.IsEmailConfirmedAsync(user); ;
            }
            catch (Exception ex)
            {
                ErrorCatch(ex, "Is Email Confirmed");
                return false;
            }
        }

        public async Task<string> GeneratePasswordResetTokenAsync(User user)
        {
            try
            {

                return await _userManager.GeneratePasswordResetTokenAsync(user); ;
            }
            catch (Exception ex)
            {
                ErrorCatch(ex, "Generate Password Reset Token");
                return "";
            }
        }

        public async Task<IdentityResult?> ResetPasswordAsync(User user,string code, string NewPassword)
        {
            try
            {
                IdentityResult identityResult = await _userManager.ResetPasswordAsync(user, code, NewPassword);

                return identityResult;
            }
            catch (Exception ex)
            {
                ErrorCatch(ex, "Reset Password");
                return IdentityResult.Failed(new IdentityError { Description = "An error occurred while Reset Password on user." });
            }
        }

        public async Task<IList<string>> GetRoles(User user)
        {
            try
            {
                IList<string> Roles = await _userManager.GetRolesAsync(user);

                return Roles;
            }
            catch (Exception ex)
            {
                ErrorCatch(ex, "Get Roles");
                return [];
            }
        }

        public IdentityError ErrorDescriberInvalidToken()
        {
            return _userManager.ErrorDescriber.InvalidToken();
        }


        private void ErrorCatch(Exception ex, string funName)
        {
            if (_logger != null)
                _logger.LogError(ex, $"An error occurred while {funName}");
            else
                throw new Exception(funName, ex);
        }
    }
}
