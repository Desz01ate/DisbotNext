using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DisbotNext.Common.Extensions
{
    public static class DiscordChannelExtensions
    {
        /// <summary>
        /// Send message that will automatically delete after timeout period.
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="message"></param>
        /// <param name="deleteInMs"></param>
        /// <returns></returns>
        public static async Task SendDisposableMessageAsync(this DiscordChannel channel, string message, int deleteInMs = 10000, CancellationToken cancellationToken = default)
        {
            var sentMessage = await channel.SendMessageAsync(message);
            Task.Delay(deleteInMs).ContinueWith(async (arg) =>
            {
                await sentMessage.DeleteAsync();
            });

        }

        /// <summary>
        /// Send file that will automatically delete after timeout period.
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="message"></param>
        /// <param name="deleteInMs"></param>
        /// <returns></returns>
        public static async Task SendDisposableFileAsync(this DiscordChannel channel, string filePath, int deleteInMs = 10000, CancellationToken cancellationToken = default)
        {
            using var fileStream = new FileStream(filePath, FileMode.Open);
            var messageBuilder = new DiscordMessageBuilder();
            messageBuilder.WithFile(Path.GetFileName(filePath), fileStream, true);
            var sentMessage = await channel.SendMessageAsync(messageBuilder);
            Task.Delay(deleteInMs).ContinueWith(async (arg) =>
            {
                await sentMessage.DeleteAsync();
            });
        }

        public static async Task SendFileAsync(this DiscordChannel channel, string filePath, bool deleteFile = false, CancellationToken cancellationToken = default)
        {
            using var fileStream = new FileStream(filePath, FileMode.Open);
            var messageBuilder = new DiscordMessageBuilder();
            messageBuilder.WithFile(Path.GetFileName(filePath), fileStream, true);
            var sentMessage = await channel.SendMessageAsync(messageBuilder);
            if (deleteFile)
                File.Delete(filePath);
        }

        public static async Task SendFileAsync(this CommandContext context, string filePath, bool deleteFile = false, CancellationToken cancellationToken = default)
        {
            using var fileStream = new FileStream(filePath, FileMode.Open);
            var messageBuilder = new DiscordMessageBuilder();
            messageBuilder.WithFile(Path.GetFileName(filePath), fileStream, true);
            var sentMessage = await context.RespondAsync(messageBuilder);
            if (deleteFile)
                File.Delete(filePath);
        }
    }
}
