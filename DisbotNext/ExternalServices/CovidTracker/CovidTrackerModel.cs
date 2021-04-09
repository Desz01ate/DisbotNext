using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisbotNext.ExternalServices.CovidTracker
{
    public class CovidTrackerModel
    {
        public string Country { get; set; }
        public int Cases { get; set; }
        public int TodayCases { get; set; }
        public int Deaths { get; set; }
        public int TodayDeaths { get; set; }
        public int Recovered { get; set; }
        public int Active { get; set; }
        public int Critical { get; set; }
        public int CasesPerOneMillion { get; set; }
    }
}
