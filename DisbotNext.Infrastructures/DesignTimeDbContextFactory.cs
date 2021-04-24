using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisbotNext.Infrastructures.Sqlite
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<SqliteDbContext>
    {
        public SqliteDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<SqliteDbContext>();
            optionsBuilder.UseSqlite($@"Data Source={Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Local.db")}");
            return new SqliteDbContext(optionsBuilder.Options);
        }
    }
}
