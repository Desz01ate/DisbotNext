using DisbotNext.Infrastructures.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace DisbotNext.Infrastructures.Common
{
    /// <summary>
    /// Abstract database context for Disbot.
    /// </summary>
    public abstract class DisbotDbContext : DbContext
    {
        public abstract DbSet<Member> Members { get; set; }
        public abstract DbSet<ChatLog> ChatLogs { get; set; }
        public abstract DbSet<TempChannel> TempChannels { get; set; }
        public abstract DbSet<ErrorLog> ErrorLogs { get; set; }
        public abstract DbSet<StockSubscription> StockSubscriptions { get; set; }
        public DisbotDbContext(DbContextOptions options) : base(options)
        {
        }
    }
}
