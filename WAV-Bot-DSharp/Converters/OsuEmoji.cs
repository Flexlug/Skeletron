using DSharpPlus;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using WAV_Osu_NetApi.Bancho.Models.Enums;
using WAV_Osu_NetApi.Gatari.Models.Enums;

namespace WAV_Bot_DSharp.Converters
{
    public class OsuEmoji
    {
        private DiscordClient client;

        public OsuEmoji(DiscordClient client)
        {
            this.client = client;
        }

        /// <summary>
        /// Get emoji, based on setted difficulty value
        /// </summary>
        /// <param name="rating"></param>
        /// <returns></returns>
        public DiscordEmoji DiffEmoji(float rating)
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

        /// <summary>
        /// Get map rank status emoji
        /// </summary>
        /// <param name="rank">Bancho rank status</param>
        /// <returns></returns>
        public DiscordEmoji RankStatusEmoji(RankStatus rank)
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

        /// <summary>
        /// Get map rank status emoji
        /// </summary>
        /// <param name="rank">Gatari rank status</param>
        /// <returns></returns>
        public DiscordEmoji RankStatusEmoji(GRankStatus rank)
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

        /// <summary>
        /// Get ranking emoji
        /// </summary>
        /// <param name="ranking">Ranking</param>
        /// <returns></returns>
        public DiscordEmoji RankingEmoji(string ranking)
        {
            switch (ranking)
            {
                case "XH":
                    return DiscordEmoji.FromGuildEmote(client, 800148121060769849);

                case "SH":
                    return DiscordEmoji.FromGuildEmote(client, 800148343534649384);

                case "X":
                    return DiscordEmoji.FromGuildEmote(client, 800147903250694216);

                case "S":
                    return DiscordEmoji.FromGuildEmote(client, 800147562673471560);

                case "A":
                    return DiscordEmoji.FromGuildEmote(client, 800147041774862340);

                case "B":
                    return DiscordEmoji.FromGuildEmote(client, 800147303124959282);

                case "C":
                    return DiscordEmoji.FromGuildEmote(client, 800147422927650847);

                case "D":
                    return DiscordEmoji.FromGuildEmote(client, 800147539797868554);

                case "F":
                    return DiscordEmoji.FromGuildEmote(client, 800182320459284501);

                default:
                    {
                        Console.WriteLine($"Couldn't find emoji for rank {ranking}");
                        return DiscordEmoji.FromGuildEmote(client, 805368650529767444);
                    }
            }
        }

        public DiscordEmoji PPEmoji() => DiscordEmoji.FromGuildEmote(client, 807875369981181972);
        public DiscordEmoji Hit300Emoji() => DiscordEmoji.FromGuildEmote(client, 800176276719271987);
        public DiscordEmoji Hit200Emoji() => DiscordEmoji.FromGuildEmote(client, 800176276542586891);
        public DiscordEmoji Hit100Emoji() => DiscordEmoji.FromGuildEmote(client, 800176276559495189);
        public DiscordEmoji Hit50Emoji() => DiscordEmoji.FromGuildEmote(client, 800176276744175657);
        public DiscordEmoji MissEmoji() => DiscordEmoji.FromGuildEmote(client, 800151438553776178);
    }
}
