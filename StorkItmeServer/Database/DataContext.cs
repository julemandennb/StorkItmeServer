

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StorkItmeServer.Model;

namespace StorkItmeServer.Database
{
    public class DataContext : IdentityDbContext<User, Role,string>
    {

        public DbSet<UserGroup> UserGroup { get; set; }
        public DbSet<StorkItme> StorkItme { get; set; }

        public DbSet<StorkItmeGroup> StorkItmeGroup { get; set; }



        public DataContext(DbContextOptions<DataContext> options):base(options)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.HasDefaultSchema("storkitmeserver");

            new DataUserGroup().ModelCreating(builder);
            new DataStorkItme().ModelCreating(builder);
            new DataStorkItmeGroup().ModelCreating(builder);

            ConfigureUuid(builder);
        }

        private void ConfigureUuid(ModelBuilder builder)
        {
            builder.Entity<UserGroup>()
                .Property(x => x.Uuid)
                .IsRequired();

            builder.Entity<StorkItme>()
                .Property(x => x.Uuid)
                .IsRequired();

            builder.Entity<StorkItmeGroup>()
                .Property(x => x.Uuid)
                .IsRequired();

        }


    }
}
