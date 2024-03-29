﻿using System;
using System.Text;

using OsuNET_Api.Models;
using OsuNET_Api.Models.Bancho;

using Microsoft.Extensions.Logging;

namespace Skeletron.Converters
{
    public class OsuEnums
    {
        private ILogger<OsuEnums> logger;

        public OsuEnums(ILogger<OsuEnums> logger)
        {
            this.logger = logger;
            logger.LogInformation("OsuEnums loaded");
        }

        /// <summary>
        /// Translate osu mods to string
        /// </summary>
        /// <param name="mods">Osu mods</param>
        /// <returns></returns>
        public string ModsToString(Mods mods)
        {
            StringBuilder sb = new StringBuilder(20);

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

        /// <summary>
        /// Получить значение OsuServer по названию сервера
        /// </summary>
        /// <param name="server"></param>
        /// <returns></returns>
        public OsuServer? StringToOsuServer(string server)
        {
            switch (server)
            {
                case "bancho":
                    return OsuServer.Bancho;

                case "gatari":
                    return OsuServer.Gatari;

                default:
                    return null;
            }
        }

        /// <summary>
        /// Получить название сервера из OsuServer
        /// </summary>
        /// <param name="server">Значение перечисления OsuServer</param>
        /// <returns></returns>
        public string OsuServerToString(OsuServer server)
        {
            switch (server)
            {
                case OsuServer.Bancho:
                    return "bancho";

                case OsuServer.Gatari:
                    return "gatari";

                default:
                    throw new Exception("Error parsing OsuServer enum");
            }
        }
    }
}
