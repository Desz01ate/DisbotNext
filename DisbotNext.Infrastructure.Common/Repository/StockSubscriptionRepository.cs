using DisbotNext.Infrastructures.Common.Models;
using DisbotNext.Infrastructures.Common.Repository.Interface;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DisbotNext.Infrastructures.Common.Repository
{
    public class StockSubscriptionRepository : IStockSubscriptionRepository
    {
        private readonly DisbotDbContext _dbContext;

        public StockSubscriptionRepository(DisbotDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        public ValueTask DeleteAsync(StockSubscription obj, CancellationToken cancellationToken = default)
        {
            this._dbContext.StockSubscriptions.Remove(obj);
            return ValueTask.CompletedTask;
        }

        public ValueTask DeleteAsync(Func<StockSubscription, bool> predicate, CancellationToken cancellationToken = default)
        {
            var subscriptions = this._dbContext.StockSubscriptions.Where(predicate);
            this._dbContext.StockSubscriptions.RemoveRange(subscriptions);
            return ValueTask.CompletedTask;
        }

        public async ValueTask<StockSubscription?> FindAsync(params object[] keys)
        {
            return await this._dbContext.StockSubscriptions.FindAsync(keys);
        }

        public IEnumerator<StockSubscription> GetEnumerator()
        {
            foreach (var record in this._dbContext.StockSubscriptions)
            {
                yield return record;
            }
        }

        public async ValueTask InsertAsync(StockSubscription obj, CancellationToken cancellationToken = default)
        {
            await this._dbContext.StockSubscriptions.AddAsync(obj);
        }

        public async ValueTask InsertManyAsync(IEnumerable<StockSubscription> source, CancellationToken cancellationToken = default)
        {
            await this._dbContext.StockSubscriptions.AddRangeAsync(source);
        }

        public ValueTask<IEnumerable<StockSubscription>> WhereAsync(Func<StockSubscription, bool> predicate, CancellationToken cancellationToken = default)
        {
            return ValueTask.FromResult(this._dbContext.StockSubscriptions.Where(predicate));
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
