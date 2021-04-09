using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.CommandsNext;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DisbotNext.Common.Configurations;
using System.Linq;
using System.Threading;

namespace DisbotNext.Common
{
    public abstract class DiscordClientAbstract : IAsyncDisposable
    {
        public readonly DiscordClient Client;

        private readonly List<DiscordChannel> _channels;

        public virtual IReadOnlyList<DiscordChannel> Channels => this._channels;

        protected DiscordClientAbstract(DiscordConfigurations configuration)
        {
            _channels = new List<DiscordChannel>();
            Client = new DiscordClient(new DiscordConfiguration()
            {
                Token = configuration.DiscordBotToken,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                Intents = DiscordIntents.All
            });
            Client.GuildDownloadCompleted += DiscordClient_GuildsDownloadCompleted;
            Client.GuildCreated += DiscordClient_GuildCreatedCompleted;
            Client.GuildDeleted += DiscordClient_GuildDeletedCompleted;
            Client.ChannelCreated += DiscordClient_ChannelCreated;
            Client.ChannelUpdated += DiscordClient_ChannelUpdated;
            Client.ChannelDeleted += DiscordClient_ChannelDeleted;
        }

        public Task ConnectAsync(CancellationToken cancellation = default)
        {
            return this.Client.ConnectAsync();
        }

        protected virtual Task DiscordClient_ChannelDeleted(DiscordClient sender, ChannelDeleteEventArgs e)
        {
            var channel = e.Channel;
            var existingChannel = _channels.SingleOrDefault(x => x.Id == channel.Id);
            if (existingChannel != null)
            {
                lock (_channels)
                    _channels.Remove(existingChannel);
            }
            return Task.CompletedTask;
        }

        protected virtual Task DiscordClient_ChannelUpdated(DiscordClient sender, ChannelUpdateEventArgs e)
        {
            var channel = e.ChannelAfter;
            if (channel.Type == ChannelType.Text && !_channels.Any(x => x.Id == channel.Id))
            {
                lock (_channels)
                    _channels.Add(channel);
            }
            return Task.CompletedTask;
        }
        protected virtual Task DiscordClient_ChannelCreated(DiscordClient sender, ChannelCreateEventArgs e)
        {
            var channel = e.Channel;
            if (channel.Type == ChannelType.Text)
            {
                lock (_channels)
                    _channels.Add(channel);
            }
            return Task.CompletedTask;
        }
        protected virtual Task DiscordClient_GuildDeletedCompleted(DiscordClient sender, GuildDeleteEventArgs e)
        {
            var guild = e.Guild;
            var channel = _channels.SingleOrDefault(x => x.GuildId == guild.Id);
            lock (_channels)
                _channels.Remove(channel);
            return Task.CompletedTask;
        }
        protected virtual Task DiscordClient_GuildCreatedCompleted(DiscordClient sender, GuildCreateEventArgs e)
        {
            var guild = e.Guild;
            var channels = guild.Channels.Select(x => x.Value);
            lock (channels)
                this._channels.AddRange(channels);
            return Task.CompletedTask;
        }
        protected virtual Task DiscordClient_GuildsDownloadCompleted(DiscordClient sender, GuildDownloadCompletedEventArgs e)
        {
            var guilds = e.Guilds;
            foreach (var guild in guilds)
            {
                var channels = guild.Value.Channels.Select(x => x.Value);
                lock (_channels)
                    this._channels.AddRange(channels);
            }
            return Task.CompletedTask;
        }

        public IEnumerable<DiscordChannel> GetDiscordChannels()
        {
            lock (_channels)
            {
                foreach (var ch in _channels)
                {
                    yield return ch;
                }
            }
        }

        public async ValueTask DisposeAsync()
        {
            await this.Client.DisconnectAsync();
        }
    }
}
