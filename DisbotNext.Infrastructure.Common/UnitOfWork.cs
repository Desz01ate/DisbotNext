using DisbotNext.Infrastructures.Common.Repository;
using DisbotNext.Infrastructures.Common.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DisbotNext.Infrastructures.Common
{
    public class UnitOfWork : IDisposable
    {
        private bool _disposed;

        private readonly DisbotDbContext dbContext;

        public UnitOfWork(DisbotDbContext dbContext)
        {
            this.dbContext = dbContext;
            this.MemberRepository = new MemberRepository(dbContext);
            this.ChatLogRepository = new ChatLogRepository(dbContext);
            this.TempChannelRepository = new TempChannelRepository(dbContext);
            this.ErrorLogRepository = new ErrorLogRepository(dbContext);
            this.StockSubscriptions = new StockSubscriptionRepository(dbContext);
        }

        public IMemberRepository MemberRepository { get; }

        public IChatLogRepository ChatLogRepository { get; }

        public ITempChannelRepository TempChannelRepository { get; }

        public IErrorLogRepository ErrorLogRepository { get; }

        public IStockSubscriptionRepository StockSubscriptions { get; }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return this.dbContext.SaveChangesAsync(cancellationToken);
        }

        protected virtual void Disposed(bool disposing)
        {
            if (!this._disposed && disposing)
            {
                this.dbContext.Dispose();
            }
            this._disposed = true;
        }

        public void Dispose()
        {
            this.Disposed(true);
            GC.SuppressFinalize(this);
        }
    }
}
