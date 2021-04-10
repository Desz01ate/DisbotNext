using DisbotNext.Infrastructures.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DisbotNext.Infrastructure.Common.Repository.Interface
{
    public interface IMemberRepository : IGenericRepository<Member>
    {
        /// <summary>
        /// Find a member in repository using id, if user is not exist then new user will be create.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        ValueTask<Member> FindOrCreateAsync(ulong id, CancellationToken cancellationToken = default);
    }
}
