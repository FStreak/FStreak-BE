using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace FStreak.API.Hubs
{
    [Authorize]
    public class StreakHub : Hub
    {
        // Có thể mở rộng các method nếu cần
    }
}
