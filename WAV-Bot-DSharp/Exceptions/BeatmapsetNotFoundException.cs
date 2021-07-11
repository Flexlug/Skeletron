using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WAV_Bot_DSharp.Exceptions
{
    public class BeatmapsetNotFoundException : Exception
    {
        public BeatmapsetNotFoundException() : base("The seearch querry returned null") { }
    }
}
