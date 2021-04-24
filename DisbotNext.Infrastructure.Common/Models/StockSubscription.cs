using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisbotNext.Infrastructures.Common.Models
{
    public class StockSubscription
    {
        public Guid Id { get; set; }

        public ulong DiscordMemberId { get; set; }

        public string Symbol { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
