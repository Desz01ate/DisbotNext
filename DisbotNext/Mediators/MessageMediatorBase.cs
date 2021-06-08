using DisbotNext.Interfaces;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DisbotNext.Mediators
{
    public abstract class MessageMediatorBase<T> : IMessageMediator<T>
    {
        protected readonly T Service;

        public MessageMediatorBase(T service)
        {
            this.Service = service ?? throw new ArgumentNullException(nameof(service));
        }

        /// <summary>
        /// Generate discord embed instance in order to use to send.
        /// </summary>
        /// <returns></returns>
        protected abstract IAsyncEnumerable<DiscordEmbed?> EnumerateDiscordEmbedAsync(string? queryString, CancellationToken cancellationToken = default);

        /// <inheritdoc/>
        public async Task SendAsync(string? queryString, Func<DiscordEmbed, Task> action, CancellationToken cancellationToken = default)
        {
            if (action == null)
                return;

            await foreach (var embed in EnumerateDiscordEmbedAsync(queryString, cancellationToken))
            {
                if (embed == null)
                    continue;
                await action(embed);
            }
        }
    }
}
