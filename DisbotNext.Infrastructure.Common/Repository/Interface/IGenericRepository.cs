﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DisbotNext.Infrastructures.Common.Repository.Interface
{
    public interface IGenericRepository<T> : IEnumerable<T>
    {
        /// <summary>
        /// Find object in repository by id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        ValueTask<T?> FindAsync(params object[] keys);

        /// <summary>
        /// Enumerate through the repository in an asynchronous manner.
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        ValueTask<IEnumerable<T>> WhereAsync(Func<T, bool> predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Delete an object from repository.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        ValueTask DeleteAsync(T obj, CancellationToken cancellationToken = default);

        /// <summary>
        /// Delete objects which are satisfy the given predicate.
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        ValueTask DeleteAsync(Func<T, bool> predicate, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Delete many objects from repository.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        ValueTask DeleteManyAsync(IEnumerable<T> source, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Insert an object to repository.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        ValueTask InsertAsync(T obj, CancellationToken cancellationToken = default);

        /// <summary>
        /// Insert many objects to repository.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        ValueTask InsertManyAsync(IEnumerable<T> source, CancellationToken cancellationToken = default);
    }
}
