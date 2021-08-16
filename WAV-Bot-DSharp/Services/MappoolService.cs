using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WAV_Bot_DSharp.Database.Interfaces;
using WAV_Bot_DSharp.Database.Models;
using WAV_Bot_DSharp.Services.Interfaces;
using WAV_Bot_DSharp.Converters;

using WAV_Osu_NetApi;
using WAV_Osu_NetApi.Models.Bancho;

using DSharpPlus.Entities;

using OsuParsers.Database.Objects;

namespace WAV_Bot_DSharp.Services
{
    public class MappoolService : IMappoolService
    {
        private IMappoolProvider mappoolProvider;
        private OsuRegex regex;

        private BanchoApi bapi;
        private GatariApi gapi;

        public MappoolService(IMappoolProvider mappoolProvider,
                              OsuRegex regex,
                              BanchoApi bapi,
                              GatariApi gapi)
        {
            this.mappoolProvider = mappoolProvider;

            this.regex = regex;

            this.bapi = bapi;
            this.gapi = gapi;
        }

        public string AddAdminMap(CompitCategories category, string url)
        {
            throw new NotImplementedException();
        }

        public string AddMap(DiscordMember member, CompitCategories category, string url)
        {
            throw new NotImplementedException();
        }

        public DiscordEmbed GetCategoryMappool(CompitCategories category)
        {
            throw new NotImplementedException();
        }

        public string RemoveMap(CompitCategories category, string bmId)
        {
            throw new NotImplementedException();
        }

        public void ResetMappool()
        {
            throw new NotImplementedException();
        }

        public string Vote(DiscordMember member, CompitCategories category, string url)
        {
            throw new NotImplementedException();
        }
    }
}
