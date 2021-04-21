using Microsoft.AspNetCore.DataProtection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YahooFinanceApi;

namespace DisbotNext.ExternalServices.Financial.Stock
{
    public class StockPriceChecker : IStockPriceChecker
    {
        public async Task<StockPrice?> GetStockPriceAsync(string symbol, CancellationToken cancellationToken = default)
        {
            var securities = await Yahoo.Symbols(symbol).Fields(Field.Symbol,
                                                                Field.LongName,
                                                                Field.RegularMarketPrice,
                                                                Field.RegularMarketOpen,
                                                                Field.RegularMarketDayLow,
                                                                Field.RegularMarketDayHigh,
                                                                Field.PostMarketPrice).QueryAsync(cancellationToken);
            if (securities.TryGetValue(symbol, out Security? security))
            {
                var sym = GetValueOrDefault<string>(security, Field.Symbol);
                var name = GetValueOrDefault<string>(security, Field.LongName);
                var regularMarketOpen = Convert.ToDecimal(GetValueOrDefault<double>(security, Field.RegularMarketOpen));
                var regularMarketPrice = Convert.ToDecimal(GetValueOrDefault<double>(security, Field.RegularMarketPrice));
                var regularMarketDayLow = Convert.ToDecimal(GetValueOrDefault<double>(security, Field.RegularMarketDayLow));
                var regularMarketDayHigh = Convert.ToDecimal(GetValueOrDefault<double>(security, Field.RegularMarketDayHigh));
                var postMarketPrice = Convert.ToDecimal(GetValueOrDefault<double>(security, Field.PostMarketPrice));
                var stock = new StockPrice()
                {
                    Symbol = sym,
                    Name = name,
                    RegularMarketOpen = regularMarketOpen,
                    RegularMarketPrice = regularMarketPrice,
                    RegularMarketDayLow = regularMarketDayLow,
                    RegularMarketDayHigh = regularMarketDayHigh,
                    PostMarketPrice = postMarketPrice
                };
                return stock;
            }

            return default;
        }

        private static T GetValueOrDefault<T>(Security security, Field field)
        {
            try
            {
                return (T)security[field];
            }
            catch
            {
                return default;
            }
        }
    }
}
