using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DisbotNext.ExternalServices.Financial.Stock
{
    public interface IStockPriceChecker
    {
        /// <summary>
        /// Get stock price of the given stock's symbol.
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<StockPrice?> GetStockPriceAsync(string symbol, CancellationToken cancellationToken = default);
    }
}
