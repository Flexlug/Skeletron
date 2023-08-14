using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

using osu.Game.Configuration;
using Skeletron.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.ModalCommands;
using DSharpPlus.ModalCommands.Attributes;

namespace Skeletron.ContextMenuCommands
{
    internal class AdminCommands : ApplicationCommandModule
    {
        private DiscordChannel LogChannel;

        public AdminCommands(DiscordClient client)
        {
            LogChannel = client.GetChannelAsync(816396082153521183).Result;
        }
        
        [ContextMenu(ApplicationCommandType.MessageContextMenu, "Delete message"), 
         RequireRoles(RoleCheckMode.Any, "Admin", "Moder", "Assistant Moder"), 
         Description("Удалить сообщение и уведомить автора об этом.")]
        public async Task DeleteMessageByLinkAndNotify(ContextMenuContext ctx)
        {
            var deletingMessage = ctx.TargetMessage;

            if (deletingMessage is null || string.IsNullOrEmpty(deletingMessage.Content))
            {
                await ctx.CreateResponseAsync("Deleting message is not specified", ephemeral: true);
                return;
            }
            
            var modal = ModalBuilder.Create("DeleteMessage")
                .WithTitle("Удаление сообщения")
                .AddComponents(new TextInputComponent("Причина удаления: ", "deleteReason", "Not stated",
                    required: true))
                .AsEphemeral();

            var interactivity = ctx.Client.GetInteractivity();
            
            await ctx.CreateResponseAsync(InteractionResponseType.Modal, modal);

            var result = await interactivity.WaitForModalAsync("DeleteMessage");

            if (result.TimedOut)
            {
                await ctx.CreateResponseAsync("Canceled", ephemeral: true);
                return;
            }

            var reason = result.Result.Values["deleteReason"];
            if (string.IsNullOrEmpty(reason))
            {
                reason = "Not stated";
            }
            
            DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
                .WithFooter($"Mod: {deletingMessage.Author.Username} {deletingMessage.Timestamp}", iconUrl: deletingMessage.Author.AvatarUrl)
                .WithDescription(deletingMessage.Content);

            if (!(deletingMessage.Author is null))
                builder.WithAuthor(name: $"From {deletingMessage.Channel.Name} by {deletingMessage.Author.Username}",
                                   iconUrl: deletingMessage.Author.AvatarUrl);

            DiscordMember user = await deletingMessage.Channel.Guild.GetMemberAsync(deletingMessage.Author.Id);
            if (!user.IsBot)
            {
                DiscordDmChannel targetChannel = await user.CreateDmChannelAsync();
                await targetChannel.SendMessageAsync(content: $"Удалено по причине: {reason}", embed: builder.Build());

                if (deletingMessage.Embeds?.Count != 0)
                    foreach (var embed in deletingMessage.Embeds)
                        await targetChannel.SendMessageAsync(embed: embed);

                if (deletingMessage.Attachments?.Count != 0)
                    foreach (var att in deletingMessage.Attachments)
                        await targetChannel.SendMessageAsync(att.Url);
            }

            await LogChannel.SendMessageAsync(
                embed: new DiscordEmbedBuilder().WithAuthor(name: deletingMessage.Author.Username, iconUrl: deletingMessage.Author.AvatarUrl)
                        .AddField("**Action**:", "delete message", true)
                        .AddField("**Violator**:", deletingMessage.Author.Mention, true)
                        .AddField("**From**:", deletingMessage.Channel.Name, true)
                        .AddField("**Reason**:", reason, true)
                        .WithFooter()
                        .Build());

            // await LogChannel.SendMessageAsync(content: $"Deleted message: \n{new string('=', 20)}\n{msg.Content}");

            if (deletingMessage.Embeds?.Count != 0)
                foreach (var embed in deletingMessage.Embeds)
                    await LogChannel.SendMessageAsync(embed: embed);

            if (deletingMessage.Attachments?.Count != 0)
                foreach (var att in deletingMessage.Attachments)
                    await LogChannel.SendMessageAsync(att.Url);

            await deletingMessage.Channel.DeleteMessageAsync(deletingMessage, reason);
        }
        //
        // [ContextMenu(ApplicationCommandType.MessageContextMenu, "Redirect and delete message"), RequireRoles(RoleCheckMode.Any, "Admin", "Moder", "Assistant Moder"), Description("Переслать сообщение в другой канал и удалить его с предыдущего.")]
        // public async Task ResendAndDeleteAsync(CommandContext commandContext,
        //     [Description("Канал, куда необходимо переслать сообщение.")] DiscordChannel targetChannel,
        //     [Description("Причина."), RemainingText] string reason = "not stated")
        // {
        //     if (commandContext.Message.Reference is null)
        //     {
        //         await commandContext.RespondAsync("Resending message is not specified");
        //         return;
        //     }
        //
        //     if (targetChannel is null)
        //     {
        //         await commandContext.RespondAsync("Target channel is not specified");
        //         return;
        //     }
        //
        //     // redirect message
        //     DiscordMessage msg = await commandContext.Channel.GetMessageAsync(commandContext.Message.Reference.Message.Id);
        //
        //     DiscordEmbedBuilder redirectedMsg = new DiscordEmbedBuilder()
        //         .WithFooter($"Mod: {commandContext.Message.Author.Username} {msg.Timestamp}", iconUrl: commandContext.Message.Author.AvatarUrl)
        //         .WithDescription(msg.Content);
        //
        //     if (!(msg.Author is null))
        //         redirectedMsg.WithAuthor(name: $"From {msg.Channel.Name} by {msg.Author.Username}",
        //                            iconUrl: msg.Author.AvatarUrl);
        //
        //     await targetChannel.SendMessageAsync(content: $"{msg.Author.Mention}\nПеренаправлено по причине: {reason}", embed: redirectedMsg.Build());
        //
        //     if (msg.Embeds?.Count != 0)
        //         foreach (var embed in msg.Embeds)
        //             await targetChannel.SendMessageAsync(embed: embed);
        //
        //     if (msg.Attachments?.Count != 0)
        //     {
        //         WebClient webClient = new WebClient();
        //         foreach (var att in msg.Attachments)
        //         {
        //             string fileName = $"{DateTime.Now.Ticks}-{att.FileName}";
        //             webClient.DownloadFile(new Uri(att.Url), $"downloads/{fileName}");
        //
        //             using (FileStream fs = new FileStream($"downloads/{fileName}", FileMode.Open))
        //                 await targetChannel.SendMessageAsync(new DiscordMessageBuilder().WithFile(fs));
        //         }
        //     }
        //
        //     // log
        //     await LogChannel.SendMessageAsync(
        //         embed: new DiscordEmbedBuilder().WithAuthor(name: commandContext.Message.Author.Username, iconUrl: commandContext.Message.Author.AvatarUrl)
        //                             .AddField("**Action**:", "resend message", true)
        //                             .AddField("**Violator**:", msg.Author.Mention, true)
        //                             .AddField("**From**:", msg.Channel.Name, true)
        //                             .AddField("**To**:", targetChannel.Name, true)
        //                             .AddField("**Reason**:", reason, true)
        //                             .WithFooter()
        //                             .Build());
        //     await msg.Channel.DeleteMessagesAsync(new[] { msg, commandContext.Message }, reason);
        //
        //     // notify in DM
        //     DiscordMember user = await msg.Channel.Guild.GetMemberAsync(msg.Author.Id);
        //     if (!user.IsBot)
        //     {
        //         DiscordDmChannel dmChannel = await user.CreateDmChannelAsync();
        //
        //         try
        //         {
        //             await dmChannel.SendMessageAsync(content: $"Перенаправлено по причине: {reason}", embed: redirectedMsg.Build());
        //         }
        //         catch (Exception ex)
        //         {
        //             logger.LogError("Error in rd command", ex);
        //         }
        //
        //         if (msg.Embeds?.Count != 0)
        //             foreach (var embed in msg.Embeds)
        //                 await dmChannel.SendMessageAsync(embed: embed);
        //
        //         if (msg.Attachments?.Count != 0)
        //             foreach (var att in msg.Attachments)
        //                 await dmChannel.SendMessageAsync(att.Url);
        //
        //     }
        //}
    }
}
