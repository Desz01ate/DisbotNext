using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisbotNext.ExternalServices.OilPriceChecker
{
    public class OilType
    {
        public string Type { get; set; }

        public string RetailName { get; set; }

        public decimal? PricePerLitre { get; set; }
    }
}
