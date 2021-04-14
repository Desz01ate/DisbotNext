using DisbotNext.Infrastructures.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisbotNext.Infrastructure.Common.Models
{
    public class ErrorLog
    {
        public int Id { get; set; }
        public string Method { get; set; }
        public string Log { get; set; }
        public DateTime CreatedAt { get; set; }
        public virtual Member TriggeredBy { get; set; }
    }
}
