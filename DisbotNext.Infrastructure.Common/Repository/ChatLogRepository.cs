using DisbotNext.Infrastructure.Common.Repository.Interface;
using DisbotNext.Infrastructures.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DisbotNext.Infrastructure.Common.Repository
{
    class ChatLogRepository : IChatLogRepository
    {
        private readonly DisbotDbContext dbContext;

        public ChatLogRepository(DisbotDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        // <inheritdoc/>
        public ValueTask DeleteAsync(ChatLog obj, CancellationToken cancellationToken = default)
        {
            this.dbContext.ChatLogs.Remove(obj);
            return ValueTask.CompletedTask;
        }

        // <inheritdoc/>
        public async ValueTask<ChatLog?> FindAsync(params object[] keys)
        {
            return await this.dbContext.ChatLogs.FindAsync(keys);
        }

        // <inheritdoc/>
        public async ValueTask InsertAsync(ChatLog obj, CancellationToken cancellationToken = default)
        {
            await this.dbContext.ChatLogs.AddAsync(obj);
        }

        // <inheritdoc/>
        public async ValueTask InsertManyAsync(IEnumerable<ChatLog> source, CancellationToken cancellationToken = default)
        {
            await this.dbContext.ChatLogs.AddRangeAsync(source.ToArray());
        }

        // <inheritdoc/>
        public ValueTask<IEnumerable<ChatLog>> WhereAsync(Func<ChatLog, bool> predicate, CancellationToken cancellationToken = default)
        {
            var result = this.dbContext.ChatLogs.Where(predicate);
            return ValueTask.FromResult(result);
        }
    }
}
