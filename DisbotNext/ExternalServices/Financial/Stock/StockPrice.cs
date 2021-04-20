using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisbotNext.ExternalServices.Financial.Stock
{
    public class StockPrice
    {
        public string Symbol { get; set; }

        public string Name { get; set; }

        public decimal? RegularMarketPrice { get; set; }

        public decimal? RegularMarketOpen { get; set; }

        public decimal? RegularMarketDayLow { get; set; }

        public decimal? RegularMarketDayHigh { get; set; }

        public decimal? PostMarketPrice { get; set; }

        public override string ToString()
        {
            var props = typeof(StockPrice).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            var sb = new StringBuilder();
            foreach (var prop in props)
            {
                var value = prop.GetValue(this);
                sb.AppendLine($"{prop.Name} = {value}");
            }
            return sb.ToString();
        }
    }
}
