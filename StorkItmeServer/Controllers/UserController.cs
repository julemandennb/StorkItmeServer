

using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using StorkItmeServer.AuthorizationHandler;
using StorkItmeServer.FromBody.StorkItme;
using StorkItmeServer.FromBody.User;
using StorkItmeServer.Help;
using StorkItmeServer.Model;
using StorkItmeServer.Model.DTO;
using StorkItmeServer.Server;
using StorkItmeServer.Server.Interface;
using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace StorkItmeServer.Controllers
{
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ILogger<StorkItmeController> _logger;
        private readonly IUserServ _userServ;
        private readonly RoleAuthorizationHandler _roleAuthorizationHandler;
        private readonly RoleManager<Role> _roleManager;
        private readonly IEmailSender<User> _emailSender;
        private readonly SignInManager<User> _signInManager;
        private readonly TimeProvider _timeProvider;
        private readonly IOptionsMonitor<BearerTokenOptions> _bearerTokenOptions;


        public UserController(ILogger<StorkItmeController> logger, IUserServ userServ,
            RoleManager<Role> roleManager,
             IEmailSender<User> emailSender, SignInManager<User> signInManager,
            TimeProvider timeProvider, IOptionsMonitor<BearerTokenOptions> optionsMonitor) {

            _logger = logger;
            _userServ = userServ;
            _roleAuthorizationHandler = new RoleAuthorizationHandler();

            _roleManager = roleManager;
            _emailSender = emailSender;
            _signInManager = signInManager;
            _timeProvider = timeProvider;
            _bearerTokenOptions = optionsMonitor;

        }


        [HttpPost("register")]
        [Authorize(Policy = "Manager")]
        public async Task<Results<Ok, ValidationProblem>> register([FromBody] RegisterRequest registration)
        {

            var email = registration.Email;

            var user = new User();
            user.UserName = email;
            user.Email = email;

            var result = await _userServ.Create(user, registration.Password);

            if (!result.Succeeded)
            {
                return CreateValidationProblem(result);
            }

            await _userServ.AddToRole(user, "Read");


            // Generate email confirmation token
            var token = await _userServ.GenerateEmailConfirmationTokenAsync(user);
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
            if (await _userServ.Get(userId) is not { } user)
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
                result = await _userServ.ConfirmEmailAsync(user, code);
            }
            else
            {
                // As with Identity UI, email and user name are one and the same. So when we update the email,
                // we need to update the user name.
                result = await _userServ.ChangeEmailAsync(user, changedEmail, code);

                if (result.Succeeded)
                {
                    result = await _userServ.SetUserNameAsync(user, changedEmail);
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

            var result = await _userServ.EmailPasswordSignInAsync(login.Email, login.Password, isPersistent, useCookieScheme, lockoutOnFailure: true, login.TwoFactorCode, login.TwoFactorRecoveryCode);

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

        [HttpPost("user/resendConfirmationEmail")]
        public async Task<Ok>  resendConfirmationEmail
        ([FromBody] ResendConfirmationEmailRequest resendRequest)
        {
            if (await _userServ.GetByEmail(resendRequest.Email) is not { } user)
            {
                return TypedResults.Ok();
            }

            // Generate email confirmation token
            var token = await _userServ.GenerateEmailConfirmationTokenAsync(user);
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
            var user = await _userServ.GetByEmail(resetRequest.Email);

            if (user is not null && await _userServ.IsEmailConfirmedAsync(user))
            {
                var code = await _userServ.GeneratePasswordResetTokenAsync(user);
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
            var user = await _userServ.GetByEmail(resetRequest.Email);

            if (user is null || !(await _userServ.IsEmailConfirmedAsync(user)))
            {
                return CreateValidationProblem(IdentityResult.Failed(_userServ.ErrorDescriberInvalidToken()));
            }

            IdentityResult result;
            try
            {
                var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(resetRequest.ResetCode));
                result = await _userServ.ResetPasswordAsync(user, code, resetRequest.NewPassword);
            }
            catch (FormatException)
            {
                result = IdentityResult.Failed(_userServ.ErrorDescriberInvalidToken());
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
                var user = await _userServ.GetByClaimsPrincipal(User);

                if (user is not null)
                {
                    var roleName = await _userServ.GetRoles(user);
                    if (roleName.Count > 0)
                    {
                        Role role = await _roleManager.FindByNameAsync(roleName.FirstOrDefault());

                        UserDTO userDTO = new UserDTO(user);

                        userDTO.Role = new RoleDTO(role);

                        userDTO.UserGroups = user.UserGroups.Select(x => new UserGroupDTO(x)).ToList();

                        userDTO.StorkItmeGroups = user.StorkItmeGroups.Select(x => new StorkItmeGroupDto(x)).ToList();

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

        [HttpGet("user/get")]
        [Authorize(Policy = "Member")]
        public async Task<IActionResult> get(string uuid)
        {
            try
            {
                var user = await _userServ.Get(uuid);

                if (user is not null)
                {
                    var roleName = await _userServ.GetRoles(user);
                    if (roleName.Count > 0)
                    {
                        Role role = await _roleManager.FindByNameAsync(roleName.FirstOrDefault());

                        UserDTO userDTO = new UserDTO(user);

                        userDTO.Role = new RoleDTO(role);

                        userDTO.UserGroups = user.UserGroups.Select(x => new UserGroupDTO(x)).ToList();

                        userDTO.StorkItmeGroups = user.StorkItmeGroups.Select(x => new StorkItmeGroupDto(x)).ToList();

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

        [HttpGet("user/GetAll")]
        [Authorize(Policy = "Manager")]
        public async Task<IActionResult> getAll(bool includeUserGroups = false, bool includeStorkItmeGroups = false, bool includeRole = false)
        {
            try
            {
                var users = await _userServ.Getall();
                if (users is not null)
                {
                    var result = new List<UserDTO>();
                    foreach (var user in users)
                    {
                        var roleName = await _userServ.GetRoles(user);
                        if (roleName.Count > 0)
                        {
                           
                            UserDTO userDTO = new UserDTO(user);
                            if (includeRole)
                            {
                                Role role = await _roleManager.FindByNameAsync(roleName.FirstOrDefault());
                                userDTO.Role = new RoleDTO(role);
                            }
                            else
                            {
                                userDTO.Role = null;
                            }
                            userDTO.UserGroups = includeUserGroups ? user.UserGroups.Select(x => new UserGroupDTO(x)).ToList() : [];
                            userDTO.StorkItmeGroups = includeStorkItmeGroups ? user.StorkItmeGroups.Select(x => new StorkItmeGroupDto(x)).ToList(): [];
                            result.Add(userDTO);
                        }
                    }
                    return Ok(result);
                }
                return StatusCode(500, "No users find");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving users.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("user/create")]
        [Authorize(Policy = "Manager")]
        public async Task<IActionResult> Create([FromBody] UserFromBody dto)
        {
            try
            {

                string role = UserHelp.Role(User);

                var isAdmin = _roleAuthorizationHandler.CheckUserRole("Admin", role);

                if(!isAdmin)
                {
                    if(dto.Role == "Admin" || dto.Role == "Manager")
                    {
                        return StatusCode(403, "You are not authorized to create a user with this role");
                    }
                }

                if (!_roleAuthorizationHandler.CheckRoleExists(role))
                {
                    return StatusCode(500, "Role don't exists");
                }


                if (string.IsNullOrEmpty(dto.Email) || string.IsNullOrEmpty(dto.Password) || string.IsNullOrEmpty(dto.ConfirmPassword))
                {
                    return BadRequest("Email, Password and ConfirmPassword are required");
                }

                if(dto.Password != dto.ConfirmPassword)
                {
                    return BadRequest("Password and ConfirmPassword do not match");
                }

                User user = new User
                {
                    Email = dto.Email,
                    UserName = dto.UserName
                };

                var result = await _userServ.Create(user, dto.Password);

                if (!result.Succeeded)
                {
                    return StatusCode(500, "Error creating user: " + string.Join(", ", result.Errors.Select(e => e.Description)));
                }

                await _userServ.AddToRole(user, dto.Role);


                // Generate email confirmation token
                var token = await _userServ.GenerateEmailConfirmationTokenAsync(user);
                var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

                // Create confirmation link
                var confirmationLink = Url.Action(
                    action: "confirmEmail",
                    controller: null,
                    values: new { userId = user.Id, code = encodedToken },
                    protocol: Request.Scheme);

                // Send confirmation email
                await SendConfirmationEmailAsync(dto.Email, confirmationLink);


                UserDTO userDTO = new UserDTO(user);

                return Ok(userDTO);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while Create user.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("user/update")]
        [Authorize(Policy = "Manager")]
        public async Task<IActionResult> Update(
            string uuid,
            [FromBody] UserFromUpdateBody dto)
        {
            try
            {
                var user = await _userServ.Get(uuid);
                var loginUser = await _userServ.GetByClaimsPrincipal(User);

                if (user is null || loginUser is null)
                    return StatusCode(500, "No user found");

                var roleNames = await _userServ.GetRoles(user);

                if (roleNames.Count == 0)
                    return StatusCode(500, "No role found");

                if (dto.Password is not null &&
                    dto.Password != dto.ConfirmPassword)
                {
                    return BadRequest("Password and ConfirmPassword do not match");
                }

                string currentRole = UserHelp.Role(User);

                var isAdmin =
                    _roleAuthorizationHandler.CheckUserRole("Admin", currentRole);

                if (!isAdmin)
                {
                    if (roleNames[0] == "Admin" ||
                        roleNames[0] == "Manager" ||
                        dto.Role == "Admin" ||
                        dto.Role == "Manager")
                    {
                        return StatusCode(
                            403,
                            "You are not authorized to update a user with this role");
                    }
                }

                user.Email = dto.Email ?? user.Email;
                user.UserName = dto.UserName ?? user.UserName;

                // Change role
                if (!string.IsNullOrWhiteSpace(dto.Role))
                {
                    var roleResult =
                        await _userServ.SetRole(user, dto.Role);

                    if (!roleResult.Succeeded)
                    {
                        return BadRequest(new
                        {
                            message = "Failed to update role",
                            errors = roleResult.Errors
                        });
                    }
                }

                // Change password
                if (!string.IsNullOrWhiteSpace(dto.Password))
                {
                    var passwordResult =
                        await _userServ.Update(user, dto.Password);

                    if (!passwordResult.Succeeded)
                    {
                        return BadRequest(new
                        {
                            message = "Failed to update password",
                            errors = passwordResult.Errors
                        });
                    }
                }

                // Update email/username
                var updateResult = await _userServ.Update(user);

                if (!updateResult.Succeeded)
                {
                    return BadRequest(new
                    {
                        message = "Failed to update user",
                        errors = updateResult.Errors
                    });
                }

                return Ok(new
                {
                    message = "User updated successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating user.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("info")]
        [Authorize(Policy = "Read")]
        public async Task<IActionResult> infoPut([FromBody] UserFromUpdateFromUserBody dto)
        {
            try
            {
                if (string.IsNullOrEmpty(dto.Password))
                {
                    return BadRequest("Password is required");
                }

                var user = await _userServ.GetByClaimsPrincipal(User);

                if (user is not null)
                {
                    var passwordCheck = await _userServ.CheckPassword(user, dto.Password);
                    if (!passwordCheck)
                    {
                        return StatusCode(500, "Password is not correct");
                    }

                    var result = await _userServ.UpdateUserAsync(user, dto);


                    if (!result.Succeeded)
                    {
                        return BadRequest(result.Errors);
                    }

                    return Ok(new
                    {
                        message = "Profile updated successfully"
                    });

                }

                return StatusCode(500, "No user find");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving user groups.");
                return StatusCode(500, "Internal server error");
            }

        }


        [HttpPost("logout")]
        public void Logud()
        {
            _signInManager.SignOutAsync();
        }

        [HttpDelete("user/delete")]
        [Authorize(Policy = "Manager")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var user = await _userServ.Get(id);
                var loginUser = await _userServ.GetByClaimsPrincipal(User);

                if (user is null || loginUser is null)
                    return StatusCode(500, "No user found");

                var roleNames = await _userServ.GetRoles(user);

                if (roleNames.Count == 0)
                    return StatusCode(500, "No role found");

                string currentRole = UserHelp.Role(User);

                var isAdmin =
                    _roleAuthorizationHandler.CheckUserRole("Admin", currentRole);

                if (!isAdmin)
                {
                    if (roleNames[0] == "Admin" ||
                        roleNames[0] == "Manager")
                    {
                        return StatusCode(
                            403,
                            "You are not authorized to update a user with this role");
                    }
                }

                var deleted = await _userServ.DeleteAsync(user);

                if (!deleted)
                    return StatusCode(500, "Could not delete user");

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while delete user.");
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
