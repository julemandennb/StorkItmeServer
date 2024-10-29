
using AspNetCore.Identity.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;
using StorkItmeServer.Database;
using StorkItmeServer.Model;
using StorkItmeServer.Overrides;
using Swashbuckle.AspNetCore.Filters;

namespace StorkItmeServer
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey
                });

                options.OperationFilter<SecurityRequirementsOperationFilter>();

            });

            builder.Services.AddDbContext<DataContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("database")));

            builder.Services.AddAuthorization();

            builder.Services.AddIdentityApiEndpoints<User>()
                .AddRoles<Role>()
                .AddEntityFrameworkStores<DataContext>();



            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();

                app.ApplyMigrations();
            }



            //app.MapIdentityApi<User>();


            app.MapIdentityApiFilterable<User,Role>(new IdentityApiEndpointRouteBuilderOptions()
            {
                ExcludeRegisterPost = false,
                ExcludeLoginPost = false,
                ExcludeRefreshPost = false,
                ExcludeConfirmEmailGet = false,
                ExcludeResendConfirmationEmailPost = false,
                ExcludeForgotPasswordPost = false,
                ExcludeResetPasswordPost = false,
                // setting ExcludeManageGroup to false will disable
                // 2FA and both Info Actions
                ExcludeManageGroup = true,
                Exclude2faPost = true,
                ExcludegInfoGet = true,
                ExcludeInfoPost = true,

            });

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            using (var scope = app.Services.CreateScope())
            {
                var roleManger = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();

                var roles = new[] { "Admin","Manager","Member","Read" };

                foreach (var role in roles)
                {
                    if(! await roleManger.RoleExistsAsync(role))
                        await roleManger.CreateAsync(new Role(role, role));
                }

            }

            using (var scope = app.Services.CreateScope())
            {
                var UserManger = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

                string email = builder.Configuration.GetSection("FirstAdminUser").GetValue<string>("email");

                string pass = builder.Configuration.GetSection("FirstAdminUser").GetValue<string>("password");

                if (email != null && pass != null)
                {

                    if (await UserManger.FindByEmailAsync(email) == null)
                    {
                        var user = new User();
                        user.UserName = email;
                        user.Email = email;

                        await UserManger.CreateAsync(user, pass);

                        await UserManger.AddToRoleAsync(user, "Admin");

                    }
                }
        

            }





            app.Run();
        }
    }
}
