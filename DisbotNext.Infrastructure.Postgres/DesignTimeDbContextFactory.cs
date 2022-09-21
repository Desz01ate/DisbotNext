using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DisbotNext.Infrastructure.Postgres
{
    internal class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<NpgsqlDbContext>
    {
        public NpgsqlDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<NpgsqlDbContext>();

            var connStr = "";

            optionsBuilder.UseNpgsql(connStr);

            return new NpgsqlDbContext(optionsBuilder.Options);
        }
    }
}
