using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

using DSharpPlus.SlashCommands;

using Microsoft.Extensions.Logging;

namespace WAV_Bot_DSharp.SlashCommands
{
    public class AbbrSlashCommands : ApplicationCommandModule
    {
        private ILogger<UserSlashCommands> logger;

        public AbbrSlashCommands(ILogger<UserSlashCommands> logger)
        {
            this.logger = logger;
        }
    }
}