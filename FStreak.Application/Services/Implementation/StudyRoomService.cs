using FStreak.Domain.Entities;
using FStreak.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using FStreak.Application.DTOs;
using FStreak.Application.Services.Interface;

namespace FStreak.Application.Services.Implementation;

public class StudyRoomService : IStudyRoomService
{
    private readonly FStreakDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<StudyRoomService> _logger;

    public StudyRoomService(
        FStreakDbContext context,
        IMapper mapper,
        ILogger<StudyRoomService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<RoomUserDto>> JoinRoom(int roomId, string userId)
    {
        try
        {
            var room = await _context.StudyRooms
                .Include(r => r.RoomUsers)
                .FirstOrDefaultAsync(r => r.StudyRoomId == roomId);

            if (room == null)
                return Result<RoomUserDto>.Failure("Room not found");

            if (room.IsPrivate)
            {
                // Check if user is invited
                var hasInvite = await _context.RoomUsers
                    .AnyAsync(ru => ru.StudyRoomId == roomId && ru.UserId == userId);
                    
                if (!hasInvite)
                    return Result<RoomUserDto>.Failure("Room is private");
            }

            var existingUser = room.RoomUsers
                .FirstOrDefault(ru => ru.UserId == userId && ru.LeftAt == null);

            if (existingUser != null)
            {
                existingUser.IsOnline = true;
                _context.RoomUsers.Update(existingUser);
            }
            else
            {
                var roomUser = new RoomUser
                {
                    StudyRoomId = roomId,
                    UserId = userId,
                    JoinedAt = DateTime.UtcNow,
                    IsOnline = true
                };
                _context.RoomUsers.Add(roomUser);
            }

            await _context.SaveChangesAsync();
            var result = await GetRoomUserDto(roomId, userId);
            return Result<RoomUserDto>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error joining room");
            return Result<RoomUserDto>.Failure("Internal server error");
        }
    }

    public async Task<Result<RoomUserDto>> LeaveRoom(int roomId, string userId)
    {
        try
        {
            var roomUser = await _context.RoomUsers
                .Include(ru => ru.User)
                .FirstOrDefaultAsync(ru => 
                    ru.StudyRoomId == roomId && 
                    ru.UserId == userId && 
                    ru.LeftAt == null);

            if (roomUser == null)
                return Result<RoomUserDto>.Failure("User not found in room");

            roomUser.LeftAt = DateTime.UtcNow;
            roomUser.IsOnline = false;
            
            var duration = roomUser.LeftAt.Value - roomUser.JoinedAt;
            roomUser.TotalStudyTime = duration;

            _context.RoomUsers.Update(roomUser);
            await _context.SaveChangesAsync();

            var result = _mapper.Map<RoomUserDto>(roomUser);
            return Result<RoomUserDto>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error leaving room");
            return Result<RoomUserDto>.Failure("Internal server error");
        }
    }

    public async Task<Result<RoomMessageDto>> AddMessage(int roomId, string userId, string content, MessageType type)
    {
        try
        {
            var roomUser = await _context.RoomUsers
                .Include(ru => ru.User)
                .FirstOrDefaultAsync(ru => 
                    ru.StudyRoomId == roomId && 
                    ru.UserId == userId && 
                    ru.IsOnline);

            if (roomUser == null)
                return Result<RoomMessageDto>.Failure("User not in room");

            var message = new RoomMessage
            {
                StudyRoomId = roomId,
                UserId = userId,
                Content = content,
                Type = type
            };

            _context.RoomMessages.Add(message);
            await _context.SaveChangesAsync();

            var result = _mapper.Map<RoomMessageDto>(message);
            result.UserName = roomUser.User.UserName ?? "Unknown";
            
            return Result<RoomMessageDto>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding message");
            return Result<RoomMessageDto>.Failure("Internal server error");
        }
    }

    public async Task HandleConnection(string userId)
    {
        var activeRooms = await _context.RoomUsers
            .Where(ru => ru.UserId == userId && ru.LeftAt == null)
            .ToListAsync();

        foreach (var room in activeRooms)
        {
            room.IsOnline = true;
        }

        await _context.SaveChangesAsync();
    }

    public async Task HandleDisconnection(string userId)
    {
        var activeRooms = await _context.RoomUsers
            .Where(ru => ru.UserId == userId && ru.IsOnline)
            .ToListAsync();

        var now = DateTime.UtcNow;
        foreach (var room in activeRooms)
        {
            room.IsOnline = false;
            room.LeftAt = now;
            room.TotalStudyTime = now - room.JoinedAt;
        }

        await _context.SaveChangesAsync();
    }

    private async Task<RoomUserDto> GetRoomUserDto(int roomId, string userId)
    {
        var roomUser = await _context.RoomUsers
            .Include(ru => ru.User)
            .FirstOrDefaultAsync(ru => 
                ru.StudyRoomId == roomId && 
                ru.UserId == userId && 
                ru.LeftAt == null);

        if (roomUser == null)
            throw new Exception("RoomUser not found");

        return new RoomUserDto
        {
            RoomUserId = roomUser.RoomUserId,
            UserId = roomUser.UserId,
            UserName = roomUser.User.UserName ?? "Unknown",
            JoinedAt = roomUser.JoinedAt,
            IsOnline = roomUser.IsOnline
        };
    }
}