using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FStreak.Application.DTOs
{
    public class AgoraTokenResponse
    {
        public string Token { get; set; }
        public string AppId { get; set; }
        public string ChannelName { get; set; }
        public string Uid { get; set; }
        public DateTime Expiration { get; set; }
    }
}
