using Microsoft.EntityFrameworkCore;
using StorkItmeServer.Model;

namespace StorkItmeServer.Database
{
    public class DataUserGroup
    {
        public void ModelCreating(ModelBuilder builder)
        {
            builder.Entity<UserGroup>(bu =>
            {
                bu.HasKey(x => x.Id);
                bu.HasIndex(p => p.Name);

                bu.HasMany(g => g.Users).WithMany(g => g.UserGroups);

            });
        }
    }
}
