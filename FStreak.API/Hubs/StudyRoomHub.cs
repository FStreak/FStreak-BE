using System.Security.Claims;
using FStreak.Application.Services.Interface;
using FStreak.Application.Services.Implementation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

using Microsoft.Extensions.Logging;
using FStreak.Domain.Entities; // Để sử dụng enum MessageType

namespace FStreak.API.Hubs;

[Authorize]
public class StudyRoomHub : Hub
{
    private readonly IStudyRoomService _roomService;
    private readonly IStreakService _streakService;
    private readonly ILogger<StudyRoomHub> _logger;
    
    public StudyRoomHub(
        IStudyRoomService roomService,
        IStreakService streakService,
        ILogger<StudyRoomHub> logger)
    {
        _roomService = roomService;
        _streakService = streakService;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrEmpty(userId))
        {
            await _roomService.HandleConnection(userId);
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrEmpty(userId))
        {
            await _roomService.HandleDisconnection(userId);
        }
        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinRoom(int roomId)
    {
        try
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                throw new HubException("User not authenticated");
            }

            var result = await _roomService.JoinRoom(roomId, userId);
            if (result.Succeeded)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, roomId.ToString());
                await Clients.Group(roomId.ToString()).SendAsync("UserJoined", result.Data);
                
                // Start tracking study time
                await _streakService.StartTracking(roomId, userId);
            }
            else
            {
                throw new HubException(result.Error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in JoinRoom");
            throw;
        }
    }

    public async Task LeaveRoom(int roomId)
    {
        try
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                throw new HubException("User not authenticated");
            }

            var result = await _roomService.LeaveRoom(roomId, userId);
            if (result.Succeeded)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId.ToString());
                await Clients.Group(roomId.ToString()).SendAsync("UserLeft", result.Data);
                
                // Stop tracking and process streak if eligible
                await _streakService.StopTracking(roomId, userId);
            }
            else
            {
                throw new HubException(result.Error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in LeaveRoom");
            throw;
        }
    }

    public async Task SendMessage(int roomId, string message)
    {
        try
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                throw new HubException("User not authenticated");
            }

            var result = await _roomService.AddMessage(roomId, userId, message, MessageType.Text);
            if (result.Succeeded)
            {
                await Clients.Group(roomId.ToString()).SendAsync("NewMessage", result.Data);
            }
            else
            {
                throw new HubException(result.Error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in SendMessage");
            throw;
        }
    }

    public async Task SendEmoji(int roomId, string emoji)
    {
        try
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                throw new HubException("User not authenticated");
            }

            var result = await _roomService.AddMessage(roomId, userId, emoji, MessageType.Emoji);
            if (result.Succeeded)
            {
                await Clients.Group(roomId.ToString()).SendAsync("NewEmoji", result.Data);
            }
            else
            {
                throw new HubException(result.Error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in SendEmoji");
            throw;
        }
    }
}