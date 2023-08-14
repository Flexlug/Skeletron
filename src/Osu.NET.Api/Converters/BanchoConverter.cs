using System;
using System.Text;
using System.Collections.Generic;

using OsuNET_Api.Models.Bancho;

namespace OsuNET_Api.Converters
{
    public static class BanchoConverter
    {
        public static string MapTypeToString(MapType type)
        {
            switch(type)
            {
                case MapType.Any:
                    return "any";
                case MapType.Ranked:
                    return "ranked";
                case MapType.Qualified:
                    return "qualified";
                case MapType.Loved:
                    return "loved";
                case MapType.Pending:
                    return "pending";
                case MapType.Graveyard:
                    return "graveyard";
                case MapType.Mine:
                    return "mine";
                default:
                    throw new InvalidCastException("Unrecognized MapType");
            }
        }
        
        /// <summary>
        /// Translate playmode string to numerical equivalent
        /// </summary>
        /// <param name="mode"></param>
        /// <remarks>Mode: 0: osu, 1: taiko, 2: ctb, 3: mania</remarks>
        /// <returns></returns>
        public static int PlaymodeStringToInt(string mode)
        {
            switch (mode)
            {   
                case "osu":
                    return 0;
                case "taiko":
                    return 1;
                case "fruits":
                    return 2;
                case "mania":
                    return 3;
                
                default:
                    throw new InvalidCastException($"Unrecognized playmode: {mode}");
            }
        }
    }
}
