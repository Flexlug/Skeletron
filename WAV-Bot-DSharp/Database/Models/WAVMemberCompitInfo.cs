using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WAV_Bot_DSharp.Database.Models
{
    public class WAVMemberCompitInfo
    {
        public bool ProvidedScore { get; set; } = false;
        public bool NonGrata { get; set; } = false;
        
        public double AvgPP { get; set; }
    }
}
