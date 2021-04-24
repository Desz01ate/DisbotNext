using DisbotNext.Infrastructures.Common.Models;
using DisbotNext.Infrastructures.Common.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DisbotNext.Infrastructures.Common.Repository
{
    public class TempChannelRepository : ITempChannelRepository
    {
        private readonly DisbotDbContext dbContext;

        public TempChannelRepository(DisbotDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public ValueTask DeleteAsync(TempChannel obj, CancellationToken cancellationToken = default)
        {
            this.dbContext.TempChannels.Remove(obj);
            return ValueTask.CompletedTask;
        }

        public async ValueTask<TempChannel?> FindAsync(params object[] keys)
        {
            return await this.dbContext.TempChannels.FindAsync(keys);
        }

        public async ValueTask InsertAsync(TempChannel obj, CancellationToken cancellationToken = default)
        {
            await this.dbContext.TempChannels.AddAsync(obj);
        }

        public async ValueTask InsertManyAsync(IEnumerable<TempChannel> source, CancellationToken cancellationToken = default)
        {
            await this.dbContext.TempChannels.AddRangeAsync(source);
        }

        public ValueTask<IEnumerable<TempChannel>> WhereAsync(Func<TempChannel, bool> predicate, CancellationToken cancellationToken = default)
        {
            return ValueTask.FromResult(this.dbContext.TempChannels.Where(predicate));
        }

        public IEnumerator<TempChannel> GetEnumerator()
        {
            foreach (var channel in this.dbContext.TempChannels)
            {
                yield return channel;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
