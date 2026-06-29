using Microsoft.EntityFrameworkCore;
using StorkItmeServer.Model;

namespace StorkItmeServer.Database
{
    public class DataStorkItmeGroup
    {
        public void ModelCreating(ModelBuilder builder)
        {
            builder.Entity<StorkItmeGroup>(bu =>
            {
                bu.HasKey(x => x.Id);
                bu.HasIndex(p => p.Name);

                bu.HasMany(g => g.Users).WithMany(g => g.StorkItmeGroups);

                bu.HasMany(e => e.StorkItmes)
                .WithOne(e => e.StorkItmeGroup)
                .HasForeignKey(e => e.StorkItmeGroupId)
                .IsRequired(false);

                bu.HasIndex(x => x.Uuid).IsUnique();

            });
        }
    }
}
