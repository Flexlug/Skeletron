using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

using WAV_Osu_NetApi.Bancho.Models;

namespace WAV_Bot_DSharp.Services.Interfaces
{
    public interface ITrackService
    {
        public Task AddTrack(User u);
    }
}
