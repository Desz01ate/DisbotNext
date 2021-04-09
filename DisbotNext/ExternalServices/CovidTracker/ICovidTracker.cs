using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DisbotNext.ExternalServices.CovidTracker
{
    public interface ICovidTracker
    {
        /// <summary>
        /// Get covid tracker status from external source API.
        /// </summary>
        /// <param name="country"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<CovidTrackerModel?> GetCovidTrackerDataAsync(string country, CancellationToken cancellationToken = default);
    }
}
