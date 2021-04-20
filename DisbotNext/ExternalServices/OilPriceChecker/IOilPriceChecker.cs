using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DisbotNext.ExternalServices.OilPriceChecker
{
    public interface IOilPriceChecker
    {
        /// <summary>
        /// Get oil price from external source API.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IEnumerable<OilRetail>> GetOilPriceAsync(CancellationToken cancellationToken = default);
    }
}
