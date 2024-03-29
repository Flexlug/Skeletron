﻿using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.Extensions.Logging;
using Skeletron.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skeletron.Commands
{
    internal class VkCommands : SkBaseCommandModule
    {
        private IVkPostToMessageService postToMessageService;
        private ILogger<VkCommands> logger;

        public VkCommands(IVkPostToMessageService postToMessageService,
                          ILogger<VkCommands> logger)
        {
            this.postToMessageService = postToMessageService;
            this.logger = logger;

            logger.LogInformation("VkCommands loaded");
        }

        [Command("vk_dummy"), Description("Send a message to a specified channel in a special guild"), Hidden]
        public async Task DummyCommand(CommandContext commandContext)
        {
            await commandContext.RespondAsync("As dummy as me");
        }
    }
}
