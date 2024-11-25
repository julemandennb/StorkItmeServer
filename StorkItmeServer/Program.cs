
using AspNetCore.Identity.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;
using StorkItmeServer.Database;
using StorkItmeServer.Handler;
using StorkItmeServer.Model;
using StorkItmeServer.Overrides;
using StorkItmeServer.Server;
using StorkItmeServer.Server.Interface;
using Swashbuckle.AspNetCore.Filters;

namespace StorkItmeServer
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.


            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigins",
                    builder => builder.AllowAnyOrigin()
                                      .AllowAnyMethod()
                                      .AllowAnyHeader());
            });


            builder.Services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
            }); 
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

            builder.Services.AddDbContext<DataContext>(options => options.UseLazyLoadingProxies().UseNpgsql(builder.Configuration.GetConnectionString("database")));

            builder.Services.AddAuthorization();

            builder.Services.AddIdentityApiEndpoints<User>()
                .AddRoles<Role>()
                .AddEntityFrameworkStores<DataContext>();

            RoleAuthorizationHandler roleAuthorizationHandler = new RoleAuthorizationHandler();

            builder.Services.AddAuthorization(options =>
            {
                foreach(string role in roleAuthorizationHandler.roleHierarchy)
                {
                    options.AddPolicy(role, policy =>
                   policy.Requirements.Add(new RoleRequirement(role)));

                }
            });

            builder.Services.AddSingleton<IAuthorizationHandler, RoleAuthorizationHandler>();

            SetUpInterface(builder.Services);



            var app = builder.Build();

            app.UseCors("AllowAllOrigins");

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();

                app.ApplyMigrations();
            }



            //app.MapIdentityApi<User>();


           /* app.MapIdentityApiFilterable<User,Role>(new IdentityApiEndpointRouteBuilderOptions()
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
                ExcludeManageGroup = false,
                Exclude2faPost = false,
                ExcludegInfoGet = false,
                ExcludeInfoPost = false,

            });*/

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            


            using (var scope = app.Services.CreateScope())
            {
                var roleManger = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();

                foreach (var role in roleAuthorizationHandler.roleHierarchy)
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


        private static void SetUpInterface(IServiceCollection services)
        {
            services.AddScoped<IStorkItmeServ, StorkItmeServ>();
            services.AddScoped<IUserGroupServ, UserGroupServ>();
            services.AddScoped<IUserServ, UserServ>();
        }



    }
}
