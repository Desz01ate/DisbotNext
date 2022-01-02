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

        private readonly List<DiscordGuild> _guilds;

        private readonly List<DiscordMember> _members;

        public virtual IReadOnlyList<DiscordChannel> Channels => this._channels;

        public virtual IReadOnlyList<DiscordGuild> Guilds => this._guilds;

        public virtual IReadOnlyList<DiscordMember> Members => this._members;

        protected DiscordClientAbstract(DiscordConfigurations configuration)
        {
            _channels = new List<DiscordChannel>();
            _guilds = new List<DiscordGuild>();
            _members = new List<DiscordMember>();
            
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
            Client.ChannelDeleted += DiscordClient_ChannelDeleted;
            Client.GuildMemberAdded += Client_GuildMemberAdded;
            Client.GuildMemberRemoved += Client_GuildMemberRemoved;
        }

        private Task Client_GuildMemberRemoved(DiscordClient sender, GuildMemberRemoveEventArgs e)
        {
            lock (_members)
            {
                _members.Remove(e.Member);
            }
            return Task.CompletedTask;
        }

        private Task Client_GuildMemberAdded(DiscordClient sender, GuildMemberAddEventArgs e)
        {
            lock (_members)
            {
                _members.Add(e.Member);
            }
            return Task.CompletedTask;
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
            lock (_channels)
                _channels.RemoveAll(x => x.GuildId == guild.Id);
            lock (_members)
                _members.RemoveAll(x => x.Guild.Id == guild.Id);
            lock (_guilds)
                _guilds.Remove(guild);
            return Task.CompletedTask;
        }
        protected virtual Task DiscordClient_GuildCreatedCompleted(DiscordClient sender, GuildCreateEventArgs e)
        {
            var guild = e.Guild;
            var channels = guild.Channels.Select(x => x.Value);
            lock (_channels)
                this._channels.AddRange(channels);
            lock (_members)
                this._members.AddRange(guild.Members.Select(x => x.Value));
            lock (_guilds)
                this._guilds.Add(guild);
            return Task.CompletedTask;
        }
        protected virtual Task DiscordClient_GuildsDownloadCompleted(DiscordClient sender, GuildDownloadCompletedEventArgs e)
        {
            var guilds = e.Guilds.Select(x => x.Value);
            foreach (var guild in guilds)
            {
                var channels = guild.Channels.Select(x => x.Value);
                lock (_channels)
                    this._channels.AddRange(channels);
                lock (_members)
                    this._members.AddRange(guild.Members.Select(x => x.Value));
                lock (_guilds)
                    this._guilds.Add(guild);
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
