using System;
using System.Collections.Generic;
using System.Text;
using WAV_Osu_NetApi.Bancho.QuerryParams;

namespace WAV_Osu_NetApi.Converters
{
    public static class BanchoConverter
    {
        public static string  MapTypeToString(MapType type)
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
    }
}
