using System;
using System.Text;
using System.Collections.Generic;

using DSharpPlus;
using DSharpPlus.Entities;

using WAV_Osu_NetApi.Bancho.Models.Enums;
using WAV_Osu_NetApi.Gatari.Models.Enums;

namespace WAV_Bot_DSharp.Converters
{
    public class OsuUtils
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

        public static DiscordEmoji RankingEmoji(string ranking, DiscordClient client)
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

        public static DiscordEmoji PPEmoji(DiscordClient client) => DiscordEmoji.FromGuildEmote(client, 807875369981181972);
        public static DiscordEmoji Hit300Emoji(DiscordClient client) => DiscordEmoji.FromGuildEmote(client, 800176276719271987);
        public static DiscordEmoji Hit200Emoji(DiscordClient client) => DiscordEmoji.FromGuildEmote(client, 800176276542586891);
        public static DiscordEmoji Hit100Emoji(DiscordClient client) => DiscordEmoji.FromGuildEmote(client, 800176276559495189); 
        public static DiscordEmoji Hit50Emoji(DiscordClient client) => DiscordEmoji.FromGuildEmote(client, 800176276744175657);
        public static DiscordEmoji MissEmoji(DiscordClient client) => DiscordEmoji.FromGuildEmote(client, 800151438553776178);

        public static string ModsToString(Mods mods)
        {
            StringBuilder sb = new StringBuilder();

            if (mods is Mods.None)
                return " NM";

            if (mods.HasFlag(Mods.NoFail))
                sb.Append(" NF");

            if (mods.HasFlag(Mods.Easy))
                sb.Append(" EZ");

            if (mods.HasFlag(Mods.TouchDevice))
                sb.Append(" TD");

            if (mods.HasFlag(Mods.Hidden))
                sb.Append(" HD");

            if (mods.HasFlag(Mods.HardRock))
                sb.Append(" HR");

            if (mods.HasFlag(Mods.SuddenDeath))
                sb.Append(" SD");

            if (mods.HasFlag(Mods.DoubleTime))
                sb.Append(" DT");

            if (mods.HasFlag(Mods.Relax))
                sb.Append(" RX");

            if (mods.HasFlag(Mods.HalfTime))
                sb.Append(" HT");

            if (mods.HasFlag(Mods.Nightcore))
                sb.Append(" NC");

            if (mods.HasFlag(Mods.Flashlight))
                sb.Append(" FL");

            if (mods.HasFlag(Mods.Autoplay))
                sb.Append(" Auto");

            if (mods.HasFlag(Mods.Relax2))
                sb.Append(" AP");

            if (mods.HasFlag(Mods.Perfect))
                sb.Append(" PF");

            if (mods.HasFlag(Mods.Key1))
                sb.Append(" K1");

            if (mods.HasFlag(Mods.Key2))
                sb.Append(" K2");

            if (mods.HasFlag(Mods.Key3))
                sb.Append(" K3");

            if (mods.HasFlag(Mods.Key4))
                sb.Append(" K4");

            if (mods.HasFlag(Mods.Key5))
                sb.Append(" K5");

            if (mods.HasFlag(Mods.Key6))
                sb.Append(" K6");

            if (mods.HasFlag(Mods.Key7))
                sb.Append(" K7");

            if (mods.HasFlag(Mods.Key8))
                sb.Append(" K8");

            if (mods.HasFlag(Mods.Key9))
                sb.Append(" K9");

            if (mods.HasFlag(Mods.FadeIn))
                sb.Append(" FI");

            if (mods.HasFlag(Mods.Cinema))
                sb.Append(" Cinema");

            if (mods.HasFlag(Mods.Random))
                sb.Append(" Random");

            if (mods.HasFlag(Mods.Target))
                sb.Append(" Target Practice");

            if (mods.HasFlag(Mods.KeyCoop))
                sb.Append(" KeyCoop");

            if (mods.HasFlag(Mods.ScoreV2))
                sb.Append(" ScoreV2");

            if (mods.HasFlag(Mods.Mirror))
                sb.Append(" Mirror");

            return sb.ToString();
        }
    }
}

