using DisbotNext.ExternalServices.OilPriceChecker;
using System.Threading;
using System.Threading.Tasks;

namespace DisbotNext.Interfaces
{
    public interface IOilPriceMessageMediator : IMessageMediator<IOilPriceChecker>
    {
        Task<bool> IsPriceChangingAsync(CancellationToken cancellationToken = default);
    }
}
