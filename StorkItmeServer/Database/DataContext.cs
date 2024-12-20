﻿

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StorkItmeServer.Model;

namespace StorkItmeServer.Database
{
    public class DataContext : IdentityDbContext<User, Role,string>
    {

        public DbSet<UserGroup> UserGroup { get; set; }
        public DbSet<StorkItme> StorkItme { get; set; }



        public DataContext(DbContextOptions<DataContext> options):base(options)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            new DataUserGroup().ModelCreating(builder);

            new DataStorkItme().ModelCreating(builder);

            builder.HasDefaultSchema("storkitmeserver");

        }


    }
}
