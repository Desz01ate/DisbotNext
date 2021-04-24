using DisbotNext.Infrastructures.Common.Repository.Interface;
using DisbotNext.Infrastructures.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DisbotNext.Infrastructures.Common.Repository
{
    internal class MemberRepository : IMemberRepository
    {
        private readonly DisbotDbContext dbContext;

        public MemberRepository(DisbotDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        // <inheritdoc/>
        public ValueTask DeleteAsync(Member obj, CancellationToken cancellationToken = default)
        {
            this.dbContext.Members.Remove(obj);
            return ValueTask.CompletedTask;
        }

        // <inheritdoc/>
        public async ValueTask<Member?> FindAsync(params object[] keys)
        {
            return await this.dbContext.Members.FindAsync(keys); ;
        }

        // <inheritdoc />
        public async ValueTask<Member> FindOrCreateAsync(ulong id, CancellationToken cancellationToken = default)
        {
            var user = await this.FindAsync(id);
            if (user == null)
            {
                user = Member.NewMember(id);
                await this.dbContext.Members.AddAsync(user);
                await this.dbContext.SaveChangesAsync();
            }
            return user;
        }

        // <inheritdoc/>
        public async ValueTask InsertAsync(Member obj, CancellationToken cancellationToken = default)
        {
            await this.dbContext.Members.AddAsync(obj);
        }

        // <inheritdoc/>
        public async ValueTask InsertManyAsync(IEnumerable<Member> source, CancellationToken cancellationToken = default)
        {
            await this.dbContext.Members.AddRangeAsync(source.ToArray());
        }

        // <inheritdoc/>
        public ValueTask<IEnumerable<Member>> WhereAsync(Func<Member, bool> predicate, CancellationToken cancellationToken = default)
        {
            var result = this.dbContext.Members.Where(predicate);
            return ValueTask.FromResult(result);
        }

        public IEnumerator<Member> GetEnumerator()
        {
            foreach (var member in this.dbContext.Members)
            {
                yield return member;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
