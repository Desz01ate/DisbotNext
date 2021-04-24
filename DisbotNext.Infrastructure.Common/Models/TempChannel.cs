using DisbotNext.Infrastructures.Common.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisbotNext.Infrastructures.Common.Models
{
    public class TempChannel
    {
        public ulong Id { get; set; }
        public Guid GroupId { get; set; }
        public ChannelType ChannelType { get; set; }
        public string ChannelName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiredAt { get; set; }
    }
}
