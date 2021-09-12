using System;

namespace WAV_Bot_DSharp.Exceptions
{
    public class BeatmapsetNotFoundException : Exception
    {
        public BeatmapsetNotFoundException() : base("The seearch querry returned null") { }
    }
}
