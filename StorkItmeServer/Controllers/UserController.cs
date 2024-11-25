

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebUtilities;
using StorkItmeServer.Handler;
using StorkItmeServer.Model;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text;
using Microsoft.AspNetCore.Authentication.BearerToken;
using System;
using Microsoft.Extensions.Options;
using StorkItmeServer.Server.Interface;
using StorkItmeServer.Server;
using StorkItmeServer.Model.DTO;

namespace StorkItmeServer.Controllers
{
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ILogger<StorkItmeController> _logger;
        private readonly IUserServ _userServ;
        private readonly RoleAuthorizationHandler _roleAuthorizationHandler;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly IUserStore<User> _userStore;
        private readonly LinkGenerator _linkGenerator;
        private readonly IEmailSender<User> _emailSender;
        private readonly SignInManager<User> _signInManager;
        private readonly TimeProvider _timeProvider;
        private readonly IOptionsMonitor<BearerTokenOptions> _bearerTokenOptions;


        public UserController(ILogger<StorkItmeController> logger, IUserServ userServ, UserManager<User> userManager,
            RoleManager<Role> roleManager, IUserStore<User> userStore,
            LinkGenerator linkGenerator, IEmailSender<User> emailSender, SignInManager<User> signInManager,
            TimeProvider timeProvider, IOptionsMonitor<BearerTokenOptions> optionsMonitor) {

            _logger = logger;
            _userServ = userServ;
            _roleAuthorizationHandler = new RoleAuthorizationHandler();
            _userManager = userManager;
            _roleManager = roleManager;
            _userStore = userStore;
            _linkGenerator = linkGenerator;
            _emailSender = emailSender;
            _signInManager = signInManager;
            _timeProvider = timeProvider;
            _bearerTokenOptions = optionsMonitor;

        }


        [HttpPost("register")]
        [Authorize(Policy = "Manager")]
        public async Task<Results<Ok, ValidationProblem>> register([FromBody] RegisterRequest registration)
        {

            var emailStore = (IUserEmailStore<User>)_userStore;
            var email = registration.Email;

            var user = new User();
            await _userStore.SetUserNameAsync(user, email, CancellationToken.None);
            await emailStore.SetEmailAsync(user, email, CancellationToken.None);
            var result = await _userManager.CreateAsync(user, registration.Password);

            if (!result.Succeeded)
            {
                return CreateValidationProblem(result);
            }

            await _userManager.AddToRoleAsync(user, "Read");


            // Generate email confirmation token
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            // Create confirmation link
            var confirmationLink = Url.Action(
                action: "confirmEmail",
                controller: null,
                values: new { userId = user.Id, code = encodedToken },
                protocol: Request.Scheme);

            // Send confirmation email
            await SendConfirmationEmailAsync(email, confirmationLink);

            return TypedResults.Ok();
        }

        [HttpGet("confirmEmail")]
        public async Task<Results<ContentHttpResult, UnauthorizedHttpResult>> confirmEmail([FromBody] string userId, [FromQuery] string code, [FromQuery] string? changedEmail) 
        {
            if (await _userManager.FindByIdAsync(userId) is not { } user)
            {
                // We could respond with a 404 instead of a 401 like Identity UI, but that feels like unnecessary information.
                return TypedResults.Unauthorized();
            }

            try
            {
                code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            }
            catch (FormatException)
            {
                return TypedResults.Unauthorized();
            }

            IdentityResult result;

            if (string.IsNullOrEmpty(changedEmail))
            {
                result = await _userManager.ConfirmEmailAsync(user, code);
            }
            else
            {
                // As with Identity UI, email and user name are one and the same. So when we update the email,
                // we need to update the user name.
                result = await _userManager.ChangeEmailAsync(user, changedEmail, code);

                if (result.Succeeded)
                {
                    result = await _userManager.SetUserNameAsync(user, changedEmail);
                }
            }

            if (!result.Succeeded)
            {
                return TypedResults.Unauthorized();
            }

            return TypedResults.Text("Thank you for confirming your email.");
        }

        [HttpPost("login")]
        public async Task<Results<Ok<AccessTokenResponse>, EmptyHttpResult, ProblemHttpResult>> login([FromBody] LoginRequest login, [FromQuery] bool? useCookies, [FromQuery] bool? useSessionCookies)
        {

            var useCookieScheme = (useCookies == true) || (useSessionCookies == true);
            var isPersistent = (useCookies == true) && (useSessionCookies != true);
            _signInManager.AuthenticationScheme = useCookieScheme ? IdentityConstants.ApplicationScheme : IdentityConstants.BearerScheme;

            var result = await _signInManager.PasswordSignInAsync(login.Email, login.Password, isPersistent, lockoutOnFailure: true);

            if (result.RequiresTwoFactor)
            {
                if (!string.IsNullOrEmpty(login.TwoFactorCode))
                {
                    result = await _signInManager.TwoFactorAuthenticatorSignInAsync(login.TwoFactorCode, isPersistent, rememberClient: isPersistent);
                }
                else if (!string.IsNullOrEmpty(login.TwoFactorRecoveryCode))
                {
                    result = await _signInManager.TwoFactorRecoveryCodeSignInAsync(login.TwoFactorRecoveryCode);
                }
            }

            if (!result.Succeeded)
            {
                return TypedResults.Problem(result.ToString(), statusCode: StatusCodes.Status401Unauthorized);
            }

            // The signInManager already produced the needed response in the form of a cookie or bearer token.
            return TypedResults.Empty;

        }


        [HttpPost("refresh")]
        public async Task<Results<Ok<AccessTokenResponse>, UnauthorizedHttpResult, SignInHttpResult, ChallengeHttpResult>> refresh
            ([FromBody] RefreshRequest refreshRequest)
        {
            var refreshTokenProtector = _bearerTokenOptions.Get(IdentityConstants.BearerScheme).RefreshTokenProtector;
            var refreshTicket = refreshTokenProtector.Unprotect(refreshRequest.RefreshToken);

            // Reject the /refresh attempt with a 401 if the token expired or the security stamp validation fails
            if (refreshTicket?.Properties?.ExpiresUtc is not { } expiresUtc ||
                _timeProvider.GetUtcNow() >= expiresUtc ||
                await _signInManager.ValidateSecurityStampAsync(refreshTicket.Principal) is not User user)

            {
                return TypedResults.Challenge();
            }

            var newPrincipal = await _signInManager.CreateUserPrincipalAsync(user);
            return TypedResults.SignIn(newPrincipal, authenticationScheme: IdentityConstants.BearerScheme);
        }

        [HttpPost("resendConfirmationEmail")]
        public async Task<Ok>  resendConfirmationEmail
        ([FromBody] ResendConfirmationEmailRequest resendRequest)
        {
            if (await _userManager.FindByEmailAsync(resendRequest.Email) is not { } user)
            {
                return TypedResults.Ok();
            }

            // Generate email confirmation token
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            // Create confirmation link
            var confirmationLink = Url.Action(
                action: "confirmEmail",
                controller: null,
                values: new { userId = user.Id, code = encodedToken },
                protocol: Request.Scheme);

            // Send confirmation email
            await SendConfirmationEmailAsync(resendRequest.Email, confirmationLink);

            return TypedResults.Ok();
        }

        [HttpPost("forgotPassword")]
        public async Task<Results<Ok, ValidationProblem>> forgotPassword
        ([FromBody] ForgotPasswordRequest resetRequest)
        {
            var user = await _userManager.FindByEmailAsync(resetRequest.Email);

            if (user is not null && await _userManager.IsEmailConfirmedAsync(user))
            {
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                await _emailSender.SendPasswordResetCodeAsync(user, resetRequest.Email, HtmlEncoder.Default.Encode(code));
            }

            return TypedResults.Ok();

        }

        [HttpPost("resetPassword")]
        [Authorize(Policy = "Read")]
        public async Task<Results<Ok, ValidationProblem>> resetPassword
        ([FromBody] ResetPasswordRequest resetRequest)
        {
            var user = await _userManager.FindByEmailAsync(resetRequest.Email);

            if (user is null || !(await _userManager.IsEmailConfirmedAsync(user)))
            {
                return CreateValidationProblem(IdentityResult.Failed(_userManager.ErrorDescriber.InvalidToken()));
            }

            IdentityResult result;
            try
            {
                var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(resetRequest.ResetCode));
                result = await _userManager.ResetPasswordAsync(user, code, resetRequest.NewPassword);
            }
            catch (FormatException)
            {
                result = IdentityResult.Failed(_userManager.ErrorDescriber.InvalidToken());
            }

            if (!result.Succeeded)
            {
                return CreateValidationProblem(result);
            }

            return TypedResults.Ok();
        }

        [HttpGet("info")]
        [Authorize(Policy = "Read")]
        public async Task<IActionResult> info()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);

                if (user is not null)
                {
                    var roleName = await _userManager.GetRolesAsync(user);
                    if (roleName.Count > 0)
                    {
                        Role role = await _roleManager.FindByNameAsync(roleName.FirstOrDefault());

                        UserDTO userDTO = new UserDTO(user);

                        userDTO.Role = new RoleDTO(role);

                        userDTO.UserGroups = user.UserGroups.Select(x => new UserGroupDTO(x)).ToList();

                        return Ok(userDTO);
                    }

                    return StatusCode(500, "No role find");
                }

                return StatusCode(500, "No user find");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving user groups.");
                return StatusCode(500, "Internal server error");
            }

        }



        private static ValidationProblem CreateValidationProblem(IdentityResult result)
        {
            // We expect a single error code and description in the normal case.
            // This could be golfed with GroupBy and ToDictionary, but perf! :P
            Debug.Assert(!result.Succeeded);
            var errorDictionary = new Dictionary<string, string[]>(1);

            foreach (var error in result.Errors)
            {
                string[] newDescriptions;

                if (errorDictionary.TryGetValue(error.Code, out var descriptions))
                {
                    newDescriptions = new string[descriptions.Length + 1];
                    Array.Copy(descriptions, newDescriptions, descriptions.Length);
                    newDescriptions[descriptions.Length] = error.Description;
                }
                else
                {
                    newDescriptions = [error.Description];
                }

                errorDictionary[error.Code] = newDescriptions;
            }

            return TypedResults.ValidationProblem(errorDictionary);
        }

        // Utility function to send the email
        private async Task SendConfirmationEmailAsync(string email, string link)
        {
            // Implement your email sending logic here, for example using an SMTP client
            /*await _emailService.SendEmailAsync(email, "Confirm Your Email",
                $"Please confirm your email by clicking this link: <a href='{link}'>link</a>");*/
        }
    }
}
