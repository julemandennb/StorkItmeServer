using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
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
        private SetDataBaseUp _setDataBaseUp;

        public UnitTestUserServ()
        {
            _setDataBaseUp = new SetDataBaseUp("UserServ");
        }

        [Fact]
        public async Task TestGet()
        {
            using (var context = _setDataBaseUp.Up("Get"))
            {
                UserServ userServ = MakeUserServ(context);

                 User user = context.Users.ToList()[1];
                Assert.NotNull(user);
                User use =  await userServ.Get(user.Id);
                Assert.NotNull(use);

                Assert.Equal(user, use);
            }
        }

        [Fact]
        public async Task TestGetByEmail()
        {
            using (var context = _setDataBaseUp.Up("TestGetByEmail"))
            {
                // Arrange
                var mockUserManager = new Mock<UserManager<User>>(
                new UserStore<User>(context),
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null
                );

                var testEmail = "test@example.com";
                var testUser = new User { Id = "1", UserName = "TestUser", Email = testEmail };

                mockUserManager
                    .Setup(um => um.FindByEmailAsync(testEmail))
                    .ReturnsAsync(testUser);

                var userServ = new UserServ(
                    null, // Inject other dependencies if necessary
                    context,
                    mockUserManager.Object,
                    null,
                    null,
                    null,
                    null,
                    null
                );

                // Act
                var result = await userServ.GetByEmail(testEmail);

                // Assert
                Assert.NotNull(result);
                Assert.Equal(testUser.Id, result.Id);
                Assert.Equal(testUser.Email, result.Email);
            }
        }

        [Fact]
        public async Task TestGetByClaimsPrincipal()
        {
            // Arrange
            using (var context = _setDataBaseUp.Up("TestGetByClaimsPrincipal"))
            {
                // Arrange
                var mockUserManager = new Mock<UserManager<User>>(
                new UserStore<User>(context),
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null
                );

                var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                {
                new Claim(ClaimTypes.Name, "testuser")
                }));

                var testUser = new User { Id = "1", UserName = "TestUser", Email = "test@example.com" };

                mockUserManager
                    .Setup(um => um.GetUserAsync(claimsPrincipal))
                    .ReturnsAsync(testUser);

                var userServ = new UserServ(
                    null, // Inject other dependencies if necessary
                    context,
                    mockUserManager.Object,
                    null,
                    null,
                    null,
                    null,
                    null
                );

                // Act
                var result = await userServ.GetByClaimsPrincipal(claimsPrincipal);

                // Assert
                Assert.NotNull(result);
                Assert.Equal(testUser.Id, result.Id);
                Assert.Equal(testUser.UserName, result.UserName);
            }
        }

        [Fact]
        public async Task TestCreate()
        {
            using (var context = _setDataBaseUp.Up("TestCreate"))
            {
                // Arrange
                var mockUserManager = new Mock<UserManager<User>>(
                new UserStore<User>(context),
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null
                );

                var testUser = new User { Id = "1", UserName = "TestUser", Email = "test@example.com" };
                var testPassword = "Test@123";

                mockUserManager
                    .Setup(um => um.CreateAsync(testUser, testPassword))
                    .ReturnsAsync(IdentityResult.Success);

                var userServ = new UserServ(
                    null, // Inject other dependencies if necessary
                    context,
                    mockUserManager.Object,
                    null,
                    null,
                    null,
                    null,
                    null
                );

                // Act
                var result = await userServ.Create(testUser, testPassword);

                // Assert
                Assert.NotNull(result);
                Assert.True(result.Succeeded);
            }
        }


        [Fact]
        public async Task TestAddToRole()
        {
            using (var context = _setDataBaseUp.Up("TestAddToRole"))
            {
                // Arrange
                var mockUserManager = new Mock<UserManager<User>>(
                new UserStore<User>(context),
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null
                );

                var mockRoleManager = new Mock<RoleManager<Role>>(
                      new RoleStore<Role>(context),
                      new IRoleValidator<Role>[0],
                      new UpperInvariantLookupNormalizer(),
                      new IdentityErrorDescriber(),
                      new LoggerFactory().CreateLogger<RoleManager<Role>>()
                );

                var testUser = new User { Id = "1", UserName = "TestUser", Email = "test@example.com" };
                var testRole = "Admin";

                mockUserManager
               .Setup(um => um.AddToRoleAsync(testUser, testRole))
               .ReturnsAsync(IdentityResult.Success);

                var userServ = new UserServ(
                    null, // Inject other dependencies if necessary
                    context,
                    mockUserManager.Object,
                    mockRoleManager.Object,
                    null,
                    null,
                    null,
                    null
                );

                // Act
                var result = await userServ.AddToRole(testUser, testRole);

                // Assert
                Assert.True(result);
            }
        }

        [Fact]
        public async Task TestGenerateEmailConfirmationToken()
        {
            // Arrange
            var mockUserManager = new Mock<UserManager<User>>(
                new Mock<IUserStore<User>>().Object, // Mock user store
                null, // Options
                null, // PasswordHasher
                null, // IUserValidators
                null, // IPasswordValidators
                null, // UpperInvariantLookupNormalizer
                null, // IdentityErrorDescriber
                null, // IServiceProvider
                null  // ILogger<UserManager<User>>
            );


            var testUser = new User { Id = "1", UserName = "TestUser", Email = "test@example.com" };
            var expectedToken = "mocked-email-confirmation-token";

            // Mock the GenerateEmailConfirmationTokenAsync method
            mockUserManager
                .Setup(um => um.GenerateEmailConfirmationTokenAsync(testUser))
                .ReturnsAsync(expectedToken);

            var userServ = new UserServ(
                null, // Inject other dependencies if necessary
                null, // DataContext (not needed for this test)
                mockUserManager.Object,
                null, // RoleManager
                null,
                null,
                null,
                null
            );

            // Act
            var result = await userServ.GenerateEmailConfirmationTokenAsync(testUser);

            // Assert
            Assert.Equal(expectedToken, result);
            mockUserManager.Verify(um => um.GenerateEmailConfirmationTokenAsync(testUser), Times.Once);
        }

        [Fact]
        public async Task TestConfirmEmail()
        {
            // Arrange
            var mockUserManager = new Mock<UserManager<User>>(
                new Mock<IUserStore<User>>().Object, // Mock user store
                null, // Options
                null, // PasswordHasher
                null, // IUserValidators
                null, // IPasswordValidators
                null, // UpperInvariantLookupNormalizer
                null, // IdentityErrorDescriber
                null, // IServiceProvider
                null  // ILogger<UserManager<User>>
            );


            var testUser = new User { Id = "1", UserName = "TestUser", Email = "test@example.com" };
            var code = "mocked-email-confirmation-token";

            // Mock the GenerateEmailConfirmationTokenAsync method
            mockUserManager
                .Setup(um => um.ConfirmEmailAsync(testUser, code))
                .ReturnsAsync(IdentityResult.Success);

            var userServ = new UserServ(
                null, // Inject other dependencies if necessary
                null, // DataContext (not needed for this test)
                mockUserManager.Object,
                null, // RoleManager
                null,
                null,
                null,
                null
            );

            // Act
            var result = await userServ.ConfirmEmailAsync(testUser, code);

            // Assert
            Assert.True(result.Succeeded);
            mockUserManager.Verify(um => um.ConfirmEmailAsync(testUser, code), Times.Once);
        }

        [Fact]
        public async Task TestChangeEmail()
        {
            // Arrange
            var mockUserManager = new Mock<UserManager<User>>(
                new Mock<IUserStore<User>>().Object, // Mock user store
                null, // Options
                null, // PasswordHasher
                null, // IUserValidators
                null, // IPasswordValidators
                null, // UpperInvariantLookupNormalizer
                null, // IdentityErrorDescriber
                null, // IServiceProvider
                null  // ILogger<UserManager<User>>
            );


            var testUser = new User { Id = "1", UserName = "TestUser", Email = "test@example.com" };
            string NewEmail = "new@example.com";
            var code = "mocked-email-confirmation-token";

            // Mock the GenerateEmailConfirmationTokenAsync method
            mockUserManager
                .Setup(um => um.ChangeEmailAsync(testUser, NewEmail, code))
                .ReturnsAsync(IdentityResult.Success);

            var userServ = new UserServ(
                null, // Inject other dependencies if necessary
                null, // DataContext (not needed for this test)
                mockUserManager.Object,
                null, // RoleManager
                null,
                null,
                null,
                null
            );

            // Act
            var result = await userServ.ChangeEmailAsync(testUser, NewEmail, code);

            // Assert
            Assert.True(result.Succeeded);
            mockUserManager.Verify(um => um.ChangeEmailAsync(testUser, NewEmail, code), Times.Once);
        }

        [Fact]
        public async Task TestSetUserName()
        {
            // Arrange
            var mockUserManager = new Mock<UserManager<User>>(
                new Mock<IUserStore<User>>().Object, // Mock user store
                null, // Options
                null, // PasswordHasher
                null, // IUserValidators
                null, // IPasswordValidators
                null, // UpperInvariantLookupNormalizer
                null, // IdentityErrorDescriber
                null, // IServiceProvider
                null  // ILogger<UserManager<User>>
            );


            var testUser = new User { Id = "1", UserName = "TestUser", Email = "test@example.com" };
            string userName = "hop";

            // Mock the GenerateEmailConfirmationTokenAsync method
            mockUserManager
                .Setup(um => um.SetUserNameAsync(testUser, userName))
                .ReturnsAsync(IdentityResult.Success);

            var userServ = new UserServ(
                null, // Inject other dependencies if necessary
                null, // DataContext (not needed for this test)
                mockUserManager.Object,
                null, // RoleManager
                null,
                null,
                null,
                null
            );

            // Act
            var result = await userServ.SetUserNameAsync(testUser, userName);

            // Assert
            Assert.True(result.Succeeded);
            mockUserManager.Verify(um => um.SetUserNameAsync(testUser, userName), Times.Once);
        }

        [Fact]
        public async Task TestIsEmailConfirmed()
        {
            // Arrange
            var mockUserManager = new Mock<UserManager<User>>(
                new Mock<IUserStore<User>>().Object, // Mock user store
                null, // Options
                null, // PasswordHasher
                null, // IUserValidators
                null, // IPasswordValidators
                null, // UpperInvariantLookupNormalizer
                null, // IdentityErrorDescriber
                null, // IServiceProvider
                null  // ILogger<UserManager<User>>
            );


            var testUser = new User { Id = "1", UserName = "TestUser", Email = "test@example.com" };

            // Mock the GenerateEmailConfirmationTokenAsync method
            mockUserManager
                .Setup(um => um.IsEmailConfirmedAsync(testUser))
                .ReturnsAsync(true);

            var userServ = new UserServ(
                null, // Inject other dependencies if necessary
                null, // DataContext (not needed for this test)
                mockUserManager.Object,
                null, // RoleManager
                null,
                null,
                null,
                null
            );

            // Act
            var result = await userServ.IsEmailConfirmedAsync(testUser);

            // Assert
            Assert.True(result);
            mockUserManager.Verify(um => um.IsEmailConfirmedAsync(testUser), Times.Once);
        }

        [Fact]
        public async Task TestGeneratePasswordResetToken()
        {
            // Arrange
            var mockUserManager = new Mock<UserManager<User>>(
                new Mock<IUserStore<User>>().Object, // Mock user store
                null, // Options
                null, // PasswordHasher
                null, // IUserValidators
                null, // IPasswordValidators
                null, // UpperInvariantLookupNormalizer
                null, // IdentityErrorDescriber
                null, // IServiceProvider
                null  // ILogger<UserManager<User>>
            );


            var testUser = new User { Id = "1", UserName = "TestUser", Email = "test@example.com" };
            string token = "tokentokentoken";
            // Mock the GenerateEmailConfirmationTokenAsync method
            mockUserManager
                .Setup(um => um.GeneratePasswordResetTokenAsync(testUser))
                .ReturnsAsync(token);

            var userServ = new UserServ(
                null, // Inject other dependencies if necessary
                null, // DataContext (not needed for this test)
                mockUserManager.Object,
                null, // RoleManager
                null,
                null,
                null,
                null
            );

            // Act
            var result = await userServ.GeneratePasswordResetTokenAsync(testUser);

            // Assert
            Assert.Equal(token,result);
            mockUserManager.Verify(um => um.GeneratePasswordResetTokenAsync(testUser), Times.Once);
        }


        [Fact]
        public async Task TestResetPassword()
        {
            // Arrange
            var mockUserManager = new Mock<UserManager<User>>(
                new Mock<IUserStore<User>>().Object, // Mock user store
                null, // Options
                null, // PasswordHasher
                null, // IUserValidators
                null, // IPasswordValidators
                null, // UpperInvariantLookupNormalizer
                null, // IdentityErrorDescriber
                null, // IServiceProvider
                null  // ILogger<UserManager<User>>
            );


            var testUser = new User { Id = "1", UserName = "TestUser", Email = "test@example.com" };
            string code = "sss";
            string NewPassword = "tokentokentoken";
            // Mock the GenerateEmailConfirmationTokenAsync method
            mockUserManager
                .Setup(um => um.ResetPasswordAsync(testUser, code, NewPassword))
                .ReturnsAsync(IdentityResult.Success);

            var userServ = new UserServ(
                null, // Inject other dependencies if necessary
                null, // DataContext (not needed for this test)
                mockUserManager.Object,
                null, // RoleManager
                null,
                null,
                null,
                null
            );

            // Act
            var result = await userServ.ResetPasswordAsync(testUser, code, NewPassword);

            // Assert
            Assert.True(result.Succeeded);
            mockUserManager.Verify(um => um.ResetPasswordAsync(testUser, code, NewPassword), Times.Once);
        }


        [Fact]
        public async Task TestGetRoles()
        {

            // Arrange
            var mockUserManager = new Mock<UserManager<User>>(
                new Mock<IUserStore<User>>().Object, // Mock user store
                null, // Options
                null, // PasswordHasher
                null, // IUserValidators
                null, // IPasswordValidators
                null, // UpperInvariantLookupNormalizer
                null, // IdentityErrorDescriber
                null, // IServiceProvider
                null  // ILogger<UserManager<User>>
            );


            var testUser = new User { Id = "1", UserName = "TestUser", Email = "test@example.com" };
            IList<string> list = new List<string> { "hop", "gg" };
            // Mock the GenerateEmailConfirmationTokenAsync method
            mockUserManager
                .Setup(um => um.GetRolesAsync(testUser))
                .ReturnsAsync(list);

            var userServ = new UserServ(
                null, // Inject other dependencies if necessary
                null, // DataContext (not needed for this test)
                mockUserManager.Object,
                null, // RoleManager
                null,
                null,
                null,
                null
            );


            var result = await userServ.GetRoles(testUser);

            Assert.Equal(list, result);
            mockUserManager.Verify(um => um.GetRolesAsync(testUser), Times.Once);
        }

        private UserServ MakeUserServ(DataContext context)
        {
            var userStore = new UserStore<User>(context);
            // Wrap IdentityOptions in IOptions<IdentityOptions>
            var identityOptions = new IdentityOptions();  // or configure as needed
            var optionsWrapper = new OptionsWrapper<IdentityOptions>(identityOptions);

            // Create an instance of UserManager with the default token provider
            var userManager = new UserManager<User>(
                userStore,
                optionsWrapper, // Correct way to pass IdentityOptions
                new PasswordHasher<User>(), // Password hasher (default)
                new List<IUserValidator<User>>(), // User validators (can be customized)
                new List<IPasswordValidator<User>>(), // Password validators (can be customized)
                new UpperInvariantLookupNormalizer(), // Default normalizer
                new IdentityErrorDescriber(), // Default error describer
                null, // Logger
                null // Token provider (with options wrapped)
            );

            var roleStore = new RoleStore<Role>(context);
            var roleManager = new RoleManager<Role>(
                roleStore,
                new IRoleValidator<Role>[0],
                new UpperInvariantLookupNormalizer(),
                new IdentityErrorDescriber(),
                new LoggerFactory().CreateLogger<RoleManager<Role>>()
            );


            UserServ userServ = new UserServ(null, context, userManager, roleManager, null, null, null, null);

            return userServ;

        }

    }
}
