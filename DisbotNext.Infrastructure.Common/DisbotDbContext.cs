using DisbotNext.Infrastructures.Common.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisbotNext.Infrastructure.Common
{
    /// <summary>
    /// Abstract database context for Disbot.
    /// </summary>
    public abstract class DisbotDbContext : DbContext
    {
        public abstract DbSet<Member> Members { get; set; }
        public abstract DbSet<ChatLog> ChatLogs { get; set; }
    }
}
