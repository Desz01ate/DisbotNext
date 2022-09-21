using DisbotNext.Infrastructure.Postgres;
using DisbotNext.Infrastructures.Common;
using Microsoft.EntityFrameworkCore;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddNpgsqlDbContext(this IServiceCollection services, string connectionString, ServiceLifetime lifetime = ServiceLifetime.Scoped)
        {
            services.AddDbContext<DisbotDbContext, NpgsqlDbContext>(options =>
            {
                options.UseNpgsql(connectionString);
            }, 
            lifetime);

            return services;
        }
    }
}
