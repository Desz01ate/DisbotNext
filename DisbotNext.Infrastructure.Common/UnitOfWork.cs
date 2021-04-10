using DisbotNext.Infrastructure.Common.Repository;
using DisbotNext.Infrastructure.Common.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DisbotNext.Infrastructure.Common
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
        }

        public IMemberRepository MemberRepository { get; }

        public IChatLogRepository ChatLogRepository { get; }

        public ITempChannelRepository TempChannelRepository { get; }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return this.dbContext.SaveChangesAsync();
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
