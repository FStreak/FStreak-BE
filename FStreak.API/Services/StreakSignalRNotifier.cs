using System;
using System.Threading.Tasks;
using FStreak.Application.Services.Interface;
using Microsoft.AspNetCore.SignalR;
using FStreak.API.Hubs;

namespace FStreak.API.Services
{
    public class StreakSignalRNotifier : IStreakRealtimeNotifier
    {
        private readonly IHubContext<StreakHub> _hubContext;
        public StreakSignalRNotifier(IHubContext<StreakHub> hubContext)
        {
            _hubContext = hubContext;
        }
        public async Task NotifyStreakCheckedIn(string userId, DateTime date)
        {
            await _hubContext.Clients.All.SendAsync("StreakCheckedIn", new { userId, date });
        }
    }
}
