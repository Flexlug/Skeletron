using System;
using System.Collections.Generic;
using System.Text;

namespace WAV_Osu_NetApi.Gatari.Models.Responses
{
    public class GUserResponse
    {
        public int code { get; set; }
        public List<GUser> users { get; set; }
    }
}
