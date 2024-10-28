using Microsoft.EntityFrameworkCore;
using StorkItmeServer.Model;

namespace StorkItmeServer.Database
{
    public class DataStorkItme
    {
        public void ModelCreating(ModelBuilder builder)
        {
            builder.Entity<StorkItme>(bu =>
            {
                bu.HasKey(x => x.Id);
                bu.HasIndex(p => p.Name);
                bu.HasIndex(p => p.Type);
                bu.HasIndex(p => p.Stork);

            });
        }
    }
}
