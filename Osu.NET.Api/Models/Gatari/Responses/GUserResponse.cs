using System;
using System.Collections.Generic;
using System.Text;

namespace OsuNET_Api.Models.Gatari.Responses
{
    public class GUserResponse
    {
        public int code { get; set; }
        public List<GUser> users { get; set; }
    }
}
