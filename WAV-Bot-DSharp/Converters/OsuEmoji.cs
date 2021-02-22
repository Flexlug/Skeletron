using System;
using System.Text;
using System.Collections.Generic;

using DSharpPlus;
using DSharpPlus.Entities;

using WAV_Osu_NetApi.Bancho.Models.Enums;
using WAV_Osu_NetApi.Gatari.Models.Enums;

namespace WAV_Bot_DSharp.Converters
{
    public class OsuEmoji
    {
        public static DiscordEmoji DiffEmoji(float rating, DiscordClient client)
        {
            // Easy
            if (rating <= 1)
                return DiscordEmoji.FromGuildEmote(client, 805376602824900648);
            else
            {
                // Normal
                if (rating <= 2.7)
                    return DiscordEmoji.FromGuildEmote(client, 805372074050322442);
                else
                {
                    // Hard
                    if (rating <= 4)
                        return DiscordEmoji.FromGuildEmote(client, 805375515593670686);
                    else
                    {
                        // Insane
                        if (rating <= 5.2)
                            return DiscordEmoji.FromGuildEmote(client, 805375873276575745);
                        else
                        {
                            // Expert
                            if (rating <= 6.3)
                                return DiscordEmoji.FromGuildEmote(client, 805377293449953330);
                            else
                            {
                                return DiscordEmoji.FromGuildEmote(client, 805377677661569065);
                            }
                        }
                    }
                }
            }
        }

        public static DiscordEmoji BanchoRankStatus(RankStatus rank, DiscordClient client)
        {
            switch (rank)
            {
                // ranked
                case RankStatus.Ranked:
                    return DiscordEmoji.FromGuildEmote(client, 805362757934383105);

                // qualified
                case RankStatus.Qualified:
                    return DiscordEmoji.FromGuildEmote(client, 805364968593686549);

                // loved
                case RankStatus.Loved:
                    return DiscordEmoji.FromGuildEmote(client, 805366123902009356);

                // other
                default:
                    return DiscordEmoji.FromGuildEmote(client, 805368650529767444);
            }
        }

        public static DiscordEmoji GatariRankStatus(GRankStatus rank, DiscordClient client)
        {
            switch (rank)
            {
                // ranked
                case GRankStatus.Ranked:
                    return DiscordEmoji.FromGuildEmote(client, 805362757934383105);

                // qualified
                case GRankStatus.Qualified:
                    return DiscordEmoji.FromGuildEmote(client, 805364968593686549);

                // loved
                case GRankStatus.Loved:
                    return DiscordEmoji.FromGuildEmote(client, 805366123902009356);

                // other
                default:
                    return DiscordEmoji.FromGuildEmote(client, 805368650529767444);
            }
        }
    }
}
