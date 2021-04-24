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
    public class ErrorLogRepository : IErrorLogRepository
    {
        private readonly DisbotDbContext dbContext;

        public ErrorLogRepository(DisbotDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public ValueTask DeleteAsync(ErrorLog obj, CancellationToken cancellationToken = default)
        {
            this.dbContext.ErrorLogs.Remove(obj);
            return ValueTask.CompletedTask;
        }

        public async ValueTask<ErrorLog?> FindAsync(params object[] keys)
        {
            return await this.dbContext.ErrorLogs.FindAsync(keys);
        }

        public IEnumerator<ErrorLog> GetEnumerator()
        {
            foreach (var obj in this.dbContext.ErrorLogs)
            {
                yield return obj;
            }
        }

        public async ValueTask InsertAsync(ErrorLog obj, CancellationToken cancellationToken = default)
        {
            await this.dbContext.ErrorLogs.AddAsync(obj);
        }

        public async ValueTask InsertManyAsync(IEnumerable<ErrorLog> source, CancellationToken cancellationToken = default)
        {
            await this.dbContext.ErrorLogs.AddRangeAsync(source.ToArray());
        }

        public ValueTask<IEnumerable<ErrorLog>> WhereAsync(Func<ErrorLog, bool> predicate, CancellationToken cancellationToken = default)
        {
            var result = this.dbContext.ErrorLogs.Where(predicate);
            return ValueTask.FromResult(result);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
