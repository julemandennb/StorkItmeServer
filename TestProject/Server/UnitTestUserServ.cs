using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using StorkItmeServer.Database;
using StorkItmeServer.Model;
using StorkItmeServer.Server;
using System.Security.Claims;

namespace TestProject.Server
{
    public class UnitTestUserServ
    {
        private readonly SetDataBaseUp _setDataBaseUp = new("UserServ");

        // -----------------------------
        // 🔧 Helpers
        // -----------------------------

        private UserServ CreateUserServ(
            DataContext context,
            Mock<UserManager<User>> userManager,
            Mock<RoleManager<Role>> roleManager,
            Mock<SignInManager<User>>? signInManagerMock = null)
        {
            var logger = new Mock<ILogger<UserServ>>();
            var emailSender = new Mock<IEmailSender<User>>();

            var signInManager = signInManagerMock ?? new Mock<SignInManager<User>>(
                userManager.Object,
                new HttpContextAccessor(),
                new Mock<IUserClaimsPrincipalFactory<User>>().Object,
                null, null, null, null
            );

            var timeProvider = TimeProvider.System;
            var options = new Mock<IOptionsMonitor<BearerTokenOptions>>();

            return new UserServ(
                logger.Object,
                context,
                userManager.Object,
                roleManager.Object,
                emailSender.Object,
                signInManager.Object,
                timeProvider,
                options.Object
            );
        }

        private Mock<UserManager<User>> MockUserManager()
        {
            return new Mock<UserManager<User>>(
                new Mock<IUserStore<User>>().Object,
                null, null, null, null, null, null, null, null
            );
        }

        private Mock<RoleManager<Role>> MockRoleManager()
        {
            return new Mock<RoleManager<Role>>(
                new Mock<IRoleStore<Role>>().Object,
                new IRoleValidator<Role>[0],
                new UpperInvariantLookupNormalizer(),
                new IdentityErrorDescriber(),
                new Logger<RoleManager<Role>>(new LoggerFactory())
            );
        }

        // -----------------------------
        // 🧪 Tests
        // -----------------------------

        [Fact]
        public async Task Get_ShouldReturnUser()
        {
            using var context = _setDataBaseUp.Up("Get");

            var userManager = MockUserManager();
            var roleManager = MockRoleManager();

            var user = context.Users.First();

            userManager
                .Setup(x => x.FindByIdAsync(user.Id))
                .ReturnsAsync(user);

            var service = CreateUserServ(context, userManager, roleManager);

            var result = await service.Get(user.Id);

            Assert.NotNull(result);
            Assert.Equal(user.Id, result!.Id);
        }

        [Fact]
        public async Task GetByEmail_ShouldReturnUser()
        {
            using var context = _setDataBaseUp.Up("Email");

            var userManager = MockUserManager();
            var roleManager = MockRoleManager();

            var user = context.Users.First();

            userManager
                .Setup(x => x.FindByEmailAsync(user.Email))
                .ReturnsAsync(user);

            var service = CreateUserServ(context, userManager, roleManager);

            var result = await service.GetByEmail(user.Email);

            Assert.NotNull(result);
            Assert.Equal(user.Email, result!.Email);
        }

        [Fact]
        public async Task GetByClaimsPrincipal_ShouldReturnUser()
        {
            using var context = _setDataBaseUp.Up("Claims");

            var userManager = MockUserManager();
            var roleManager = MockRoleManager();

            var user = context.Users.First();

            var claims = new ClaimsPrincipal(
                new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, user.UserName)
                })
            );

            userManager
                .Setup(x => x.GetUserAsync(claims))
                .ReturnsAsync(user);

            var service = CreateUserServ(context, userManager, roleManager);

            var result = await service.GetByClaimsPrincipal(claims);

            Assert.NotNull(result);
            Assert.Equal(user.Id, result!.Id);
        }


        [Fact]
        public async Task GetAll_ByIds_ShouldReturnFilteredUsers()
        {
            using var context = _setDataBaseUp.Up("GetAllByIds");

            var userManager = MockUserManager();
            var roleManager = MockRoleManager();

            var users = context.Users.ToList();
            List<string> ids = new List<string> { users[0].Id };

            var service = CreateUserServ(context, userManager, roleManager);

            // Act
            var result = await service.Getall(ids);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(ids.Count, result.Count);
        }

        [Fact]
        public async Task GetAll_ShouldReturnAllUsers()
        {
            using var context = _setDataBaseUp.Up("GetAll");

            var userManager = MockUserManager();
            var roleManager = MockRoleManager();

            var service = CreateUserServ(context, userManager, roleManager);

            // Act
            var result = await service.Getall();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Count >= 2);
        }

        [Fact]
        public async Task Create_ShouldReturnSuccess()
        {
            using var context = _setDataBaseUp.Up("Create");

            var userManager = MockUserManager();
            var roleManager = MockRoleManager();

            var user = new User { UserName = "Test", Email = "test@test.dk" };

            userManager
                .Setup(x => x.CreateAsync(user, "Password123!"))
                .ReturnsAsync(IdentityResult.Success);

            var service = CreateUserServ(context, userManager, roleManager);

            var result = await service.Create(user, "Password123!");

            Assert.NotNull(result);
            Assert.True(result!.Succeeded);
        }

        [Fact]
        public async Task AddToRole_ShouldReturnTrue()
        {
            using var context = _setDataBaseUp.Up("Role");

            var userManager = MockUserManager();
            var roleManager = MockRoleManager();

            var user = context.Users.First();

            userManager
                .Setup(x => x.AddToRoleAsync(user, "Admin"))
                .ReturnsAsync(IdentityResult.Success);

            var service = CreateUserServ(context, userManager, roleManager);

            var result = await service.AddToRole(user, "Admin");

            Assert.True(result);
        }

        [Fact]
        public async Task GetRoles_ShouldReturnRoles()
        {
            using var context = _setDataBaseUp.Up("Roles");

            var userManager = MockUserManager();
            var roleManager = MockRoleManager();

            var user = context.Users.First();

            var roles = new List<string> { "Admin", "User" };

            userManager
                .Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(roles);

            var service = CreateUserServ(context, userManager, roleManager);

            var result = await service.GetRoles(user);

            Assert.Equal(roles, result);
        }

        [Fact]
        public async Task GenerateEmailToken_ShouldReturnToken()
        {
            using var context = _setDataBaseUp.Up("Token");

            var userManager = MockUserManager();
            var roleManager = MockRoleManager();

            var user = context.Users.First();

            userManager
                .Setup(x => x.GenerateEmailConfirmationTokenAsync(user))
                .ReturnsAsync("token123");

            var service = CreateUserServ(context, userManager, roleManager);

            var result = await service.GenerateEmailConfirmationTokenAsync(user);

            Assert.Equal("token123", result);
        }

        [Fact]
        public async Task ResetPassword_ShouldReturnSuccess()
        {
            using var context = _setDataBaseUp.Up("Reset");

            var userManager = MockUserManager();
            var roleManager = MockRoleManager();

            var user = context.Users.First();

            userManager
                .Setup(x => x.ResetPasswordAsync(user, "code", "newpass"))
                .ReturnsAsync(IdentityResult.Success);

            var service = CreateUserServ(context, userManager, roleManager);

            var result = await service.ResetPasswordAsync(user, "code", "newpass");

            Assert.True(result!.Succeeded);
        }

        [Fact]
        public async Task ConfirmEmail_ShouldReturnSuccess()
        {
            using var context = _setDataBaseUp.Up("ConfirmEmail");

            var userManager = MockUserManager();
            var roleManager = MockRoleManager();

            var user = context.Users.First();

            userManager
                .Setup(x => x.ConfirmEmailAsync(user, "code123"))
                .ReturnsAsync(IdentityResult.Success);

            var service = CreateUserServ(context, userManager, roleManager);

            var result = await service.ConfirmEmailAsync(user, "code123");

            Assert.NotNull(result);
            Assert.True(result!.Succeeded);
        }

        [Fact]
        public async Task ChangeEmail_ShouldReturnSuccess()
        {
            using var context = _setDataBaseUp.Up("ChangeEmail");

            var userManager = MockUserManager();
            var roleManager = MockRoleManager();

            var user = context.Users.First();

            userManager
                .Setup(x => x.ChangeEmailAsync(user, "new@mail.com", "code123"))
                .ReturnsAsync(IdentityResult.Success);

            var service = CreateUserServ(context, userManager, roleManager);

            var result = await service.ChangeEmailAsync(user, "new@mail.com", "code123");

            Assert.NotNull(result);
            Assert.True(result!.Succeeded);
        }

        [Fact]
        public async Task SetUserName_ShouldReturnSuccess()
        {
            using var context = _setDataBaseUp.Up("SetUserName");

            var userManager = MockUserManager();
            var roleManager = MockRoleManager();

            var user = context.Users.First();

            userManager
                .Setup(x => x.SetUserNameAsync(user, "newName"))
                .ReturnsAsync(IdentityResult.Success);

            var service = CreateUserServ(context, userManager, roleManager);

            var result = await service.SetUserNameAsync(user, "newName");

            Assert.NotNull(result);
            Assert.True(result!.Succeeded);
        }

        [Fact]
        public async Task IsEmailConfirmed_ShouldReturnTrue()
        {
            using var context = _setDataBaseUp.Up("IsEmailConfirmed");

            var userManager = MockUserManager();
            var roleManager = MockRoleManager();

            var user = context.Users.First();

            userManager
                .Setup(x => x.IsEmailConfirmedAsync(user))
                .ReturnsAsync(true);

            var service = CreateUserServ(context, userManager, roleManager);

            var result = await service.IsEmailConfirmedAsync(user);

            Assert.True(result);
        }

        [Fact]
        public async Task GeneratePasswordResetToken_ShouldReturnToken()
        {
            using var context = _setDataBaseUp.Up("PasswordToken");

            var userManager = MockUserManager();
            var roleManager = MockRoleManager();

            var user = context.Users.First();

            userManager
                .Setup(x => x.GeneratePasswordResetTokenAsync(user))
                .ReturnsAsync("reset-token");

            var service = CreateUserServ(context, userManager, roleManager);

            var result = await service.GeneratePasswordResetTokenAsync(user);

            Assert.Equal("reset-token", result);
        }

        [Fact]
        public async Task UpdateUserAsync_ShouldReturnSuccess_WhenNoPasswordChange()
        {
            using var context = _setDataBaseUp.Up("UpdateNoPwd");

            var userManager = MockUserManager();
            var roleManager = MockRoleManager();

            var user = context.Users.First();

            userManager.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

            var service = CreateUserServ(context, userManager, roleManager);

            var dto = new StorkItmeServer.FromBody.User.UserFromUpdateFromUserBody { Password = "old", NewPassword = null };

            var result = await service.UpdateUserAsync(user, dto);

            Assert.True(result.Succeeded);
        }

        [Fact]
        public async Task UpdateUserAsync_ShouldReturnFailed_WhenChangePasswordFails()
        {
            using var context = _setDataBaseUp.Up("UpdatePwdFail");

            var userManager = MockUserManager();
            var roleManager = MockRoleManager();

            var user = context.Users.First();

            userManager.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);
            userManager.Setup(x => x.ChangePasswordAsync(user, "old", "newpwd")).ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "bad" }));

            var service = CreateUserServ(context, userManager, roleManager);

            var dto = new StorkItmeServer.FromBody.User.UserFromUpdateFromUserBody { Password = "old", NewPassword = "newpwd" };

            var result = await service.UpdateUserAsync(user, dto);

            Assert.False(result.Succeeded);
        }

        [Fact]
        public async Task CheckPassword_ReturnsFalse_WhenUserNullOrPasswordEmpty()
        {
            using var context = _setDataBaseUp.Up("CheckPwd");

            var userManager = MockUserManager();
            var roleManager = MockRoleManager();

            var service = CreateUserServ(context, userManager, roleManager);

            Assert.False(await service.CheckPassword(null, "pwd"));
            Assert.False(await service.CheckPassword(context.Users.First(), ""));
        }

        [Fact]
        public async Task CheckPassword_ReturnsTrue_WhenUserManagerReturnsTrue()
        {
            using var context = _setDataBaseUp.Up("CheckPwd2");

            var userManager = MockUserManager();
            var roleManager = MockRoleManager();

            var user = context.Users.First();

            userManager.Setup(x => x.CheckPasswordAsync(user, "pwd")).ReturnsAsync(true);

            var service = CreateUserServ(context, userManager, roleManager);

            Assert.True(await service.CheckPassword(user, "pwd"));
        }

        [Fact]
        public async Task EmailPasswordSignIn_UserNotFound_ReturnsFailed()
        {
            using var context = _setDataBaseUp.Up("SignIn1");

            var userManager = MockUserManager();
            var roleManager = MockRoleManager();

            userManager.Setup(x => x.FindByEmailAsync("noone@test.com")).ReturnsAsync((User?)null);

            var service = CreateUserServ(context, userManager, roleManager);

            var result = await service.EmailPasswordSignInAsync("noone@test.com", "pwd", false, true, false);

            Assert.False(result.Succeeded);
        }

        [Fact]
        public async Task EmailPasswordSignIn_PasswordSignInSuccess_ReturnsSuccess()
        {
            using var context = _setDataBaseUp.Up("SignIn2");

            var userManager = MockUserManager();
            var roleManager = MockRoleManager();

            var signInManager = new Mock<SignInManager<User>>(userManager.Object, new HttpContextAccessor(), new Mock<IUserClaimsPrincipalFactory<User>>().Object, null, null, null, null);

            var user = context.Users.First();

            userManager.Setup(x => x.FindByEmailAsync(user.Email)).ReturnsAsync(user);
            signInManager.Setup(x => x.PasswordSignInAsync(user, "pwd", false, false)).ReturnsAsync(SignInResult.Success);

            var service = CreateUserServ(context, userManager, roleManager, signInManager);

            var result = await service.EmailPasswordSignInAsync(user.Email, "pwd", false, true, false);

            Assert.True(result.Succeeded);
        }

        [Fact]
        public async Task EmailPasswordSignIn_TwoFactor_WithCode_ReturnsSuccess()
        {
            using var context = _setDataBaseUp.Up("SignIn3");

            var userManager = MockUserManager();
            var roleManager = MockRoleManager();

            var signInManager = new Mock<SignInManager<User>>(userManager.Object, new HttpContextAccessor(), new Mock<IUserClaimsPrincipalFactory<User>>().Object, null, null, null, null);

            var user = context.Users.First();

            userManager.Setup(x => x.FindByEmailAsync(user.Email)).ReturnsAsync(user);
            signInManager.Setup(x => x.PasswordSignInAsync(user, "pwd", false, false)).ReturnsAsync(SignInResult.TwoFactorRequired);
            signInManager.Setup(x => x.TwoFactorAuthenticatorSignInAsync("123456", false, It.IsAny<bool>())).ReturnsAsync(SignInResult.Success);

            var service = CreateUserServ(context, userManager, roleManager, signInManager);

            var result = await service.EmailPasswordSignInAsync(user.Email, "pwd", false, true, false, "123456", "");

            Assert.True(result.Succeeded);
        }

        [Fact]
        public async Task Update_ShouldUpdateUser_WhenNoPasswordProvided()
        {
            using var context = _setDataBaseUp.Up("UpdateNoPassword");

            var userManager = MockUserManager();
            var roleManager = MockRoleManager();

            var user = context.Users.First();

            userManager
                .Setup(x => x.UpdateAsync(user))
                .ReturnsAsync(IdentityResult.Success);

            var service = CreateUserServ(context, userManager, roleManager);

            var result = await service.Update(user);

            Assert.NotNull(result);
            Assert.True(result!.Succeeded);

            userManager.Verify(
                x => x.UpdateAsync(user),
                Times.Once);

            userManager.Verify(
                x => x.RemovePasswordAsync(It.IsAny<User>()),
                Times.Never);

            userManager.Verify(
                x => x.AddPasswordAsync(It.IsAny<User>(), It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task Update_ShouldChangePasswordAndUpdateUser_WhenPasswordProvided()
        {
            using var context = _setDataBaseUp.Up("UpdateWithPassword");

            var userManager = MockUserManager();
            var roleManager = MockRoleManager();

            var user = context.Users.First();

            userManager
                .Setup(x => x.RemovePasswordAsync(user))
                .ReturnsAsync(IdentityResult.Success);

            userManager
                .Setup(x => x.AddPasswordAsync(user, "NewPassword123!"))
                .ReturnsAsync(IdentityResult.Success);

            userManager
                .Setup(x => x.UpdateAsync(user))
                .ReturnsAsync(IdentityResult.Success);

            var service = CreateUserServ(context, userManager, roleManager);

            var result = await service.Update(user, "NewPassword123!");

            Assert.NotNull(result);
            Assert.True(result!.Succeeded);

            userManager.Verify(
                x => x.RemovePasswordAsync(user),
                Times.Once);

            userManager.Verify(
                x => x.AddPasswordAsync(user, "NewPassword123!"),
                Times.Once);

            userManager.Verify(
                x => x.UpdateAsync(user),
                Times.Once);
        }

        [Fact]
        public async Task Update_ShouldReturnFailure_WhenRemovePasswordFails()
        {
            using var context = _setDataBaseUp.Up("UpdateRemovePasswordFail");

            var userManager = MockUserManager();
            var roleManager = MockRoleManager();

            var user = context.Users.First();

            var failure = IdentityResult.Failed(
                new IdentityError { Description = "Could not remove password" });

            userManager
                .Setup(x => x.RemovePasswordAsync(user))
                .ReturnsAsync(failure);

            var service = CreateUserServ(context, userManager, roleManager);

            var result = await service.Update(user, "NewPassword123!");

            Assert.NotNull(result);
            Assert.False(result!.Succeeded);

            userManager.Verify(
                x => x.RemovePasswordAsync(user),
                Times.Once);

            userManager.Verify(
                x => x.AddPasswordAsync(It.IsAny<User>(), It.IsAny<string>()),
                Times.Never);

            userManager.Verify(
                x => x.UpdateAsync(It.IsAny<User>()),
                Times.Never);
        }

        [Fact]
        public async Task Update_ShouldReturnFailure_WhenAddPasswordFails()
        {
            using var context = _setDataBaseUp.Up("UpdateAddPasswordFail");

            var userManager = MockUserManager();
            var roleManager = MockRoleManager();

            var user = context.Users.First();

            userManager
                .Setup(x => x.RemovePasswordAsync(user))
                .ReturnsAsync(IdentityResult.Success);

            var failure = IdentityResult.Failed(
                new IdentityError { Description = "Could not add password" });

            userManager
                .Setup(x => x.AddPasswordAsync(user, "NewPassword123!"))
                .ReturnsAsync(failure);

            var service = CreateUserServ(context, userManager, roleManager);

            var result = await service.Update(user, "NewPassword123!");

            Assert.NotNull(result);
            Assert.False(result!.Succeeded);

            userManager.Verify(
                x => x.RemovePasswordAsync(user),
                Times.Once);

            userManager.Verify(
                x => x.AddPasswordAsync(user, "NewPassword123!"),
                Times.Once);

            userManager.Verify(
                x => x.UpdateAsync(It.IsAny<User>()),
                Times.Never);
        }

        [Fact]
        public async Task Update_ShouldReturnFailure_WhenUpdateAsyncFails()
        {
            using var context = _setDataBaseUp.Up("UpdateUserFail");

            var userManager = MockUserManager();
            var roleManager = MockRoleManager();

            var user = context.Users.First();

            var failure = IdentityResult.Failed(
                new IdentityError { Description = "Update failed" });

            userManager
                .Setup(x => x.UpdateAsync(user))
                .ReturnsAsync(failure);

            var service = CreateUserServ(context, userManager, roleManager);

            var result = await service.Update(user);

            Assert.NotNull(result);
            Assert.False(result!.Succeeded);

            userManager.Verify(
                x => x.UpdateAsync(user),
                Times.Once);
        }

        [Fact]
        public async Task Update_ShouldReturnFailure_WhenExceptionOccurs()
        {
            using var context = _setDataBaseUp.Up("UpdateException");

            var userManager = MockUserManager();
            var roleManager = MockRoleManager();

            var user = context.Users.First();

            userManager
                .Setup(x => x.UpdateAsync(user))
                .ThrowsAsync(new Exception("Database error"));

            var service = CreateUserServ(context, userManager, roleManager);

            var result = await service.Update(user);

            Assert.NotNull(result);
            Assert.False(result!.Succeeded);

            Assert.Contains(
                result.Errors,
                x => x.Description == "An error occurred while Update the user.");
        }

        [Fact]
        public async Task SetRole_ShouldAddRole_WhenUserHasNoRoles()
        {
            using var context = _setDataBaseUp.Up("SetRoleNoRoles");

            var userManager = MockUserManager();
            var roleManager = MockRoleManager();

            var user = context.Users.First();

            userManager
                .Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string>());

            userManager
                .Setup(x => x.AddToRoleAsync(user, "Admin"))
                .ReturnsAsync(IdentityResult.Success);

            var service = CreateUserServ(context, userManager, roleManager);

            var result = await service.SetRole(user, "Admin");

            Assert.True(result.Succeeded);

            userManager.Verify(
                x => x.GetRolesAsync(user),
                Times.Once);

            userManager.Verify(
                x => x.RemoveFromRolesAsync(
                    It.IsAny<User>(),
                    It.IsAny<IEnumerable<string>>()),
                Times.Never);

            userManager.Verify(
                x => x.AddToRoleAsync(user, "Admin"),
                Times.Once);
        }

        [Fact]
        public async Task SetRole_ShouldRemoveOldRolesAndAddNewRole()
        {
            using var context = _setDataBaseUp.Up("SetRoleExisting");

            var userManager = MockUserManager();
            var roleManager = MockRoleManager();

            var user = context.Users.First();

            var currentRoles = new List<string>
            {
                "User",
                "Moderator"
            };

            userManager
                .Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(currentRoles);

            userManager
                .Setup(x => x.RemoveFromRolesAsync(user, currentRoles))
                .ReturnsAsync(IdentityResult.Success);

            userManager
                .Setup(x => x.AddToRoleAsync(user, "Admin"))
                .ReturnsAsync(IdentityResult.Success);

            var service = CreateUserServ(context, userManager, roleManager);

            var result = await service.SetRole(user, "Admin");

            Assert.True(result.Succeeded);

            userManager.Verify(
                x => x.GetRolesAsync(user),
                Times.Once);

            userManager.Verify(
                x => x.RemoveFromRolesAsync(user, currentRoles),
                Times.Once);

            userManager.Verify(
                x => x.AddToRoleAsync(user, "Admin"),
                Times.Once);
        }

        [Fact]
        public async Task SetRole_ShouldReturnFailure_WhenRemovingRolesFails()
        {
            using var context = _setDataBaseUp.Up("SetRoleRemoveFail");

            var userManager = MockUserManager();
            var roleManager = MockRoleManager();

            var user = context.Users.First();

            var currentRoles = new List<string>
            {
                "User"
            };

            var failure = IdentityResult.Failed(
                new IdentityError { Description = "Remove role failed" });

            userManager
                .Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(currentRoles);

            userManager
                .Setup(x => x.RemoveFromRolesAsync(user, currentRoles))
                .ReturnsAsync(failure);

            var service = CreateUserServ(context, userManager, roleManager);

            var result = await service.SetRole(user, "Admin");

            Assert.False(result.Succeeded);

            userManager.Verify(
                x => x.RemoveFromRolesAsync(user, currentRoles),
                Times.Once);

            userManager.Verify(
                x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task SetRole_ShouldReturnFailure_WhenAddingNewRoleFails()
        {
            using var context = _setDataBaseUp.Up("SetRoleAddFail");

            var userManager = MockUserManager();
            var roleManager = MockRoleManager();

            var user = context.Users.First();

            userManager
                .Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string>());

            var failure = IdentityResult.Failed(
                new IdentityError { Description = "Add role failed" });

            userManager
                .Setup(x => x.AddToRoleAsync(user, "Admin"))
                .ReturnsAsync(failure);

            var service = CreateUserServ(context, userManager, roleManager);

            var result = await service.SetRole(user, "Admin");

            Assert.False(result.Succeeded);

            userManager.Verify(
                x => x.AddToRoleAsync(user, "Admin"),
                Times.Once);
        }

        [Fact]
        public async Task SetRole_ShouldReturnFailure_WhenExceptionOccurs()
        {
            using var context = _setDataBaseUp.Up("SetRoleException");

            var userManager = MockUserManager();
            var roleManager = MockRoleManager();

            var user = context.Users.First();

            userManager
                .Setup(x => x.GetRolesAsync(user))
                .ThrowsAsync(new Exception("Database error"));

            var service = CreateUserServ(context, userManager, roleManager);

            var result = await service.SetRole(user, "Admin");

            Assert.False(result.Succeeded);

            Assert.Contains(
                result.Errors,
                x => x.Description ==
                    "An error occurred while updating the user role.");
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnTrue_WhenDeleteSucceeds()
        {
            using var context = _setDataBaseUp.Up("Delete");

            var userManager = MockUserManager();
            var roleManager = MockRoleManager();

            var user = context.Users.First();

            userManager
                .Setup(x => x.DeleteAsync(user))
                .ReturnsAsync(IdentityResult.Success);

            var service = CreateUserServ(context, userManager, roleManager);

            var result = await service.DeleteAsync(user);

            Assert.True(result);

            userManager.Verify(
                x => x.DeleteAsync(user),
                Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnFalse_WhenExceptionOccurs()
        {
            using var context = _setDataBaseUp.Up("DeleteException");

            var userManager = MockUserManager();
            var roleManager = MockRoleManager();

            var user = context.Users.First();

            userManager
                .Setup(x => x.DeleteAsync(user))
                .ThrowsAsync(new Exception("Database error"));

            var service = CreateUserServ(context, userManager, roleManager);

            var result = await service.DeleteAsync(user);

            Assert.False(result);

            userManager.Verify(
                x => x.DeleteAsync(user),
                Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnFalse_WhenDeleteFails()
        {
            using var context = _setDataBaseUp.Up("DeleteFailed");

            var userManager = MockUserManager();
            var roleManager = MockRoleManager();

            var user = context.Users.First();

            var failure = IdentityResult.Failed(
                new IdentityError
                {
                    Description = "Delete failed"
                });

            userManager
                .Setup(x => x.DeleteAsync(user))
                .ReturnsAsync(failure);

            var service = CreateUserServ(context, userManager, roleManager);

            var result = await service.DeleteAsync(user);

            Assert.False(result);

            userManager.Verify(
                x => x.DeleteAsync(user),
                Times.Once);
        }

    }
}