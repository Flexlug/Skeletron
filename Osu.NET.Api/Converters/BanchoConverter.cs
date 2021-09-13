using System;
using System.Text;
using System.Collections.Generic;

using OsuNET_Api.Models.Bancho;

namespace OsuNET_Api.Converters
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
