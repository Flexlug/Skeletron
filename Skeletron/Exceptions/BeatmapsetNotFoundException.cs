using System;

namespace Skeletron.Exceptions
{
    public class BeatmapsetNotFoundException : Exception
    {
        public BeatmapsetNotFoundException() : base("The seearch querry returned null") { }
    }
}
