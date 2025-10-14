using AutoMapper;
using FStreak.Application.DTOs;
using FStreak.Application.Services.Interface;
using FStreak.Domain.Entities;
using FStreak.Domain.Interfaces;
using FStreak.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;

namespace FStreak.Application.Services.Implementation;

public class StudyRoomService : IStudyRoomService
{
    private readonly FStreakDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<StudyRoomService> _logger;
    private readonly IUserRepository _userRepo;
    private readonly IAgoraService _agoraService;

    public StudyRoomService(
        FStreakDbContext context,
        IMapper mapper,
        ILogger<StudyRoomService> logger,
        IUserRepository userRepo,
        IAgoraService agoraService)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _userRepo = userRepo;
        _agoraService = agoraService;
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

            // Map thủ công thay vì dùng AutoMapper
            var result = new RoomUserDto
            {
                RoomUserId = roomUser.RoomUserId,
                UserId = roomUser.UserId,
                UserName = roomUser.User?.UserName ?? "Unknown",
                JoinedAt = roomUser.JoinedAt,
                IsOnline = roomUser.IsOnline
            };

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

            // Map thủ công thay vì dùng AutoMapper
            var result = new RoomMessageDto
            {
                RoomMessageId = message.RoomMessageId,
                UserId = message.UserId,
                UserName = roomUser.User?.UserName ?? "Unknown",
                Content = message.Content,
                Type = message.Type,
                CreatedAt = message.CreatedAt
            };

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


    //Triển khai thêm các method mới:

    public async Task<Result<RoomUserDto>> JoinRoomByCodeAsync(string roomCode, string userId)
    {
        try
        {
            var room = await _context.StudyRooms
                .Include(r => r.RoomUsers)
                .FirstOrDefaultAsync(r => r.InviteCode == roomCode);

            if (room == null)
                return Result<RoomUserDto>.Failure("Room not found");

            if (room.IsPrivate && room.InviteCode != roomCode)
            {
                // Check if user is invited
                var hasInvite = await _context.RoomUsers
                    .AnyAsync(ru => ru.StudyRoomId == room.StudyRoomId && ru.UserId == userId);

                if (!hasInvite)
                    return Result<RoomUserDto>.Failure("Room is private");
            }

            // Delegate to existing JoinRoom method
            return await JoinRoom(room.StudyRoomId, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error joining room by code");
            return Result<RoomUserDto>.Failure("Internal server error");
        }
    }

    public async Task<Result<StudyRoomDto>> CreateRoomAsync(CreateRoomDto createDto, string userId)
    {
        try
        {
            //viết lại hàm get user by id vì userId là string, còn repository là int
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null)
                return Result<StudyRoomDto>.Failure("User not found. Please login to create room.");

            //Tạo mã mời ngẫu nhiên
            string inviteCode = GenerateInviteCode();

            // Tạo phòng học mới
            var room = new StudyRoom
            {
                Name = createDto.Name,
                Description = createDto.Description,
                IsPrivate = createDto.IsPrivate,
                InviteCode = inviteCode, // Tạo mã mời ngẫu nhiên (sử dụng range operator thay vì Substring)
                CreatedById = userId,
                StartTime = createDto.StartTime,
                EndTime = createDto.EndTime,
                IsActive = true,
                MeetingLink = null, // Có thể cập nhật sau // GENERATE MEETING LINK
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };
            _context.StudyRooms.Add(room);
            await _context.SaveChangesAsync();

            //Tạo meeting link thông qua Agora
            var channelName = $"room-{room.StudyRoomId}";
            var tokenResult = await _agoraService.GenerateTokenAsync(channelName, userId);

            if(tokenResult.Succeeded && tokenResult.Data != null)
            {
                //Cập nhật meeting link
                room.MeetingLink = $"agora://{tokenResult.Data.AppId}/{channelName}";
                _context.StudyRooms.Update(room);
                await _context.SaveChangesAsync();

            }

            //thêm người tạo phfong là người đầu tiên
            var roomUser = new RoomUser
            {
                StudyRoomId = room.StudyRoomId,
                UserId = userId, //lưu userId của người tạo
                JoinedAt = DateTime.UtcNow,
                IsOnline = true,
            };
            _context.RoomUsers.Add(roomUser);
            await _context.SaveChangesAsync();

            //Tạo response
            var result = await GetRoomByIdWithDetails(room.StudyRoomId);
            if (result == null)
                return Result<StudyRoomDto>.Failure("Failed to retrieve created room");
            return Result<StudyRoomDto>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating room");
            return Result<StudyRoomDto>.Failure($"Failed to create room: {ex.Message}");
        }

    }

    public async Task<Result<StudyRoomDto>> GetRoomByCodeAsync(string roomCode)
    {
        try
        {
            var room = await _context.StudyRooms
                .Include(r => r.CreatedBy)
                .Include(r => r.RoomUsers)
                    .ThenInclude(ru => ru.User)
                .FirstOrDefaultAsync(r => r.InviteCode == roomCode); //InviteCode là roomCode

            if (room == null)
                return Result<StudyRoomDto>.Failure("Room not found");

            var roomDto = new StudyRoomDto
            {
                StudyRoomId = room.StudyRoomId,
                Name = room.Name,
                Description = room.Description,
                IsPrivate = room.IsPrivate,
                InviteCode = room.InviteCode,
                IsActive = room.IsActive,
                MeetingLink = room.MeetingLink,
                StartTime = room.StartTime,
                EndTime = room.EndTime,
                CreatedById = room.CreatedById,
                CreatedBy = room.CreatedBy != null ? new UserDto
                {
                    FirstName = room.CreatedBy.FirstName,
                    LastName = room.CreatedBy.LastName
                } : null,
                RoomUsers = room.RoomUsers.Select(ru => new RoomUserDto
                {
                    RoomUserId = ru.RoomUserId,
                    UserId = ru.UserId,
                    UserName = ru.User?.UserName ?? "Unknown",
                    JoinedAt = ru.JoinedAt,
                    IsOnline = ru.IsOnline
                }).ToList()
            };
            return Result<StudyRoomDto>.Success(roomDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting room by code");
            return Result<StudyRoomDto>.Failure("Internal server error");
        }
    }

    public async Task<Result<StudyRoomDto>> GetRoomByIdAsync(int roomId)
    {
        try
        {
            var room = await GetRoomByIdWithDetails(roomId);

            if (room == null)
                return Result<StudyRoomDto>.Failure("Room not found");

            return Result<StudyRoomDto>.Success(room);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting room by ID");
            return Result<StudyRoomDto>.Failure("Internal server error");
        }
    }

    public async Task<Result<List<StudyRoomDto>>> GetActiveRoomsAsync()
    {
        try
        {
            var now = DateTime.UtcNow;
            var rooms = await _context.StudyRooms
                .Include(r => r.CreatedBy)
                .Include(r => r.RoomUsers)
                    .ThenInclude(ru => ru.User)
                .Where(r => r.IsActive && r.EndTime > now)
                .ToListAsync();

            // Map thủ công thay vì dùng AutoMapper
            var roomDtos = rooms.Select(room => new StudyRoomDto
            {
                StudyRoomId = room.StudyRoomId,
                Name = room.Name,
                Description = room.Description,
                IsPrivate = room.IsPrivate,
                InviteCode = room.InviteCode,
                IsActive = room.IsActive,
                MeetingLink = room.MeetingLink,
                StartTime = room.StartTime,
                EndTime = room.EndTime,
                CreatedById = room.CreatedById,
                CreatedBy = room.CreatedBy != null ? new UserDto
                {
                    FirstName = room.CreatedBy.FirstName,
                    LastName = room.CreatedBy.LastName
                } : null,
                RoomUsers = room.RoomUsers.Select(ru => new RoomUserDto
                {
                    RoomUserId = ru.RoomUserId,
                    UserId = ru.UserId,
                    UserName = ru.User?.UserName ?? "Unknown",
                    JoinedAt = ru.JoinedAt,
                    IsOnline = ru.IsOnline
                }).ToList()
            }).ToList();
            return Result<List<StudyRoomDto>>.Success(roomDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active rooms");
            return Result<List<StudyRoomDto>>.Failure("Internal server error");
        }
    }

    public async Task<Result<bool>> EndRoomAsync(int roomId, string userId)
    {
        try
        {
            var room = await _context.StudyRooms
                .FirstOrDefaultAsync(r => r.StudyRoomId == roomId && r.CreatedById == userId);

            if (room == null)
                return Result<bool>.Failure("Room not found or you are not the creator");

            room.IsActive = false;
            room.UpdatedAt = DateTime.UtcNow;

            // Set all users in the room to offline and calculate study time
            var roomUsers = await _context.RoomUsers
                .Where(ru => ru.StudyRoomId == roomId && ru.LeftAt == null)
                .ToListAsync();

            var now = DateTime.UtcNow;
            foreach (var user in roomUsers)
            {
                user.IsOnline = false;
                user.LeftAt = now;
                user.TotalStudyTime = now - user.JoinedAt;
            }

            await _context.SaveChangesAsync();
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ending room");
            return Result<bool>.Failure("Internal server error");
        }
    }

    // Helper methods
    private async Task<StudyRoomDto> GetRoomByIdWithDetails(int roomId)
    {
        var room = await _context.StudyRooms
            .Include(r => r.CreatedBy)
            .Include(r => r.RoomUsers) // Bỏ filter ở đây
                .ThenInclude(ru => ru.User)
            .FirstOrDefaultAsync(r => r.StudyRoomId == roomId);

        if (room == null)
            return null;

        // Map thủ công và filter sau khi load
        var roomDto = new StudyRoomDto
        {
            StudyRoomId = room.StudyRoomId,
            Name = room.Name,
            Description = room.Description,
            IsPrivate = room.IsPrivate,
            InviteCode = room.InviteCode,
            IsActive = room.IsActive,
            MeetingLink = room.MeetingLink,
            StartTime = room.StartTime,
            EndTime = room.EndTime,
            CreatedById = room.CreatedById,
            CreatedBy = room.CreatedBy != null ? new UserDto
            {
                FirstName = room.CreatedBy.FirstName,
                LastName = room.CreatedBy.LastName
            } : null,
            // Filter ở đây thay vì trong Include
            RoomUsers = room.RoomUsers
                .Where(ru => ru.LeftAt == null)
                .Select(ru => new RoomUserDto
                {
                    RoomUserId = ru.RoomUserId,
                    UserId = ru.UserId,
                    UserName = ru.User?.UserName ?? "Unknown",
                    JoinedAt = ru.JoinedAt,
                    IsOnline = ru.IsOnline
                }).ToList()
        };

        return roomDto;
    }

    private string GenerateInviteCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        var code = new StringBuilder(8);

        for (int i = 0; i < 8; i++)
        {
            code.Append(chars[random.Next(chars.Length)]);
        }

        return code.ToString();
    }
}