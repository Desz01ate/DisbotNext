using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisbotNext.ExternalServices.OilPriceChecker
{
    public class OilRetail
    {
        public string RetailName { get; set; }

        public IEnumerable<OilType> Types { get; set; }
    }
}
