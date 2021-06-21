using DisbotNext.Infrastructures.Common;
using DisbotNext.Infrastructures.Sqlite;
using Microsoft.EntityFrameworkCore;
namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSqliteDbContext(this IServiceCollection services, string connectionString, ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            services.AddDbContext<DisbotDbContext, SqliteDbContext>(o =>
            {
                o.UseSqlite(connectionString);
            }, lifetime);
            return services;
        }
    }
}
