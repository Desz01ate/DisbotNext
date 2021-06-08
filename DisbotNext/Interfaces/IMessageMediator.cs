using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DisbotNext.Interfaces
{
    /// <summary>
    /// Generic interface for message mediator.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IMessageMediator<T>
    {
        /// <summary>
        /// Compose a mesage builder and send to given action in an asynchronous manner.
        /// </summary>
        /// <param name="queryString">Optional query string (not required).</param>
        /// <param name="action">Action to perform after the service has been called.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns></returns>
        Task SendAsync(string? queryString, Func<DiscordEmbed, Task> action, CancellationToken cancellationToken = default);
    }
}
