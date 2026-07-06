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
            Mock<RoleManager<Role>> roleManager)
        {
            var logger = new Mock<ILogger<UserServ>>();
            var emailSender = new Mock<IEmailSender<User>>();

            var signInManager = new Mock<SignInManager<User>>(
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
    }
}