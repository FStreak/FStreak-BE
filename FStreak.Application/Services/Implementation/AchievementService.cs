using FStreak.Application.DTOs;
using FStreak.Application.Services.Interface;
using FStreak.Domain.Entities;
using FStreak.Domain.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace FStreak.Application.Services.Implementation
{
    public class AchievementService : IAchievementService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AchievementService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<List<UserAchievementDto>>> GetUserAchievementsAsync(string userId)
        {
            try
            {
                var userAchievements = await _unitOfWork.Achievements.GetUserAchievementsAsync(userId);
                var dtos = _mapper.Map<List<UserAchievementDto>>(userAchievements);
                return Result<List<UserAchievementDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                return Result<List<UserAchievementDto>>.Failure($"Failed to get user achievements: {ex.Message}");
            }
        }

        public async Task<Result<UserAchievementDto>> GetUserAchievementAsync(string userId, Guid achievementId)
        {
            try
            {
                var userAchievement = await _unitOfWork.Achievements.GetUserAchievementAsync(userId, achievementId);
                if (userAchievement == null)
                {
                    return Result<UserAchievementDto>.Failure("User achievement not found");
                }

                var dto = _mapper.Map<UserAchievementDto>(userAchievement);
                return Result<UserAchievementDto>.Success(dto);
            }
            catch (Exception ex)
            {
                return Result<UserAchievementDto>.Failure($"Failed to get user achievement: {ex.Message}");
            }
        }

        public async Task<Result<UserAchievementDto>> ClaimAchievementAsync(string userId, Guid achievementId)
        {
            try
            {
                var userAchievement = await _unitOfWork.Achievements.GetUserAchievementAsync(userId, achievementId);
                if (userAchievement == null)
                {
                    return Result<UserAchievementDto>.Failure("User achievement not found");
                }

                if (userAchievement.IsClaimed)
                {
                    return Result<UserAchievementDto>.Failure("Achievement already claimed");
                }

                userAchievement.IsClaimed = true;
                userAchievement.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.UserAchievements.UpdateAsync(userAchievement);
                await _unitOfWork.SaveChangesAsync();

                var dto = _mapper.Map<UserAchievementDto>(userAchievement);
                return Result<UserAchievementDto>.Success(dto);
            }
            catch (Exception ex)
            {
                return Result<UserAchievementDto>.Failure($"Failed to claim achievement: {ex.Message}");
            }
        }

        public async Task<Result<UserAchievementDto>> AwardAchievementAsync(string userId, string achievementCode, string? progress = null)
        {
            try
            {
                // Check if achievement exists
                var achievement = await _unitOfWork.Achievements.GetByCodeAsync(achievementCode);
                if (achievement == null || !achievement.IsActive)
                {
                    return Result<UserAchievementDto>.Failure("Achievement not found or inactive");
                }

                // Check if user already has this achievement
                var existing = await _unitOfWork.Achievements.GetUserAchievementAsync(userId, achievement.Id);
                if (existing != null)
                {
                    return Result<UserAchievementDto>.Success(_mapper.Map<UserAchievementDto>(existing));
                }

                // Create new user achievement
                var userAchievement = new UserAchievement
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    AchievementId = achievement.Id,
                    EarnedAt = DateTime.UtcNow,
                    Progress = progress,
                    IsClaimed = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _unitOfWork.UserAchievements.AddAsync(userAchievement);
                await _unitOfWork.SaveChangesAsync();

                // Reload with navigation properties
                var reloaded = await _unitOfWork.Achievements.GetUserAchievementAsync(userId, achievement.Id);
                var dto = _mapper.Map<UserAchievementDto>(reloaded!);

                return Result<UserAchievementDto>.Success(dto);
            }
            catch (Exception ex)
            {
                return Result<UserAchievementDto>.Failure($"Failed to award achievement: {ex.Message}");
            }
        }

        public async Task<Result<List<AchievementDto>>> GetAllAchievementsAsync()
        {
            try
            {
                var achievements = await _unitOfWork.Achievements.GetActiveAchievementsAsync();
                var dtos = _mapper.Map<List<AchievementDto>>(achievements);
                return Result<List<AchievementDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                return Result<List<AchievementDto>>.Failure($"Failed to get achievements: {ex.Message}");
            }
        }

        public async Task<Result<AchievementDto>> GetAchievementByIdAsync(Guid id)
        {
            try
            {
                var achievement = await _unitOfWork.Achievements.GetByIdAsync(id);
                if (achievement == null)
                {
                    return Result<AchievementDto>.Failure("Achievement not found");
                }

                var dto = _mapper.Map<AchievementDto>(achievement);
                return Result<AchievementDto>.Success(dto);
            }
            catch (Exception ex)
            {
                return Result<AchievementDto>.Failure($"Failed to get achievement: {ex.Message}");
            }
        }

        public async Task<Result<AchievementDto>> CreateAchievementAsync(CreateAchievementDto dto)
        {
            try
            {
                // Check if code already exists
                var exists = await _unitOfWork.Achievements.ExistsByCodeAsync(dto.Code);
                if (exists)
                {
                    return Result<AchievementDto>.Failure("Achievement code already exists");
                }

                var achievement = _mapper.Map<Achievement>(dto);
                achievement.Id = Guid.NewGuid();
                achievement.CreatedAt = DateTime.UtcNow;
                achievement.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.Achievements.AddAsync(achievement);
                await _unitOfWork.SaveChangesAsync();

                var result = await _unitOfWork.Achievements.GetByIdAsync(achievement.Id);
                var achievementDto = _mapper.Map<AchievementDto>(result!);

                return Result<AchievementDto>.Success(achievementDto);
            }
            catch (Exception ex)
            {
                return Result<AchievementDto>.Failure($"Failed to create achievement: {ex.Message}");
            }
        }

        public async Task<Result<AchievementDto>> UpdateAchievementAsync(Guid id, UpdateAchievementDto dto)
        {
            try
            {
                var achievement = await _unitOfWork.Achievements.GetByIdAsync(id);
                if (achievement == null)
                {
                    return Result<AchievementDto>.Failure("Achievement not found");
                }

                if (dto.Name != null) achievement.Name = dto.Name;
                if (dto.Description != null) achievement.Description = dto.Description;
                if (dto.Points.HasValue) achievement.Points = dto.Points.Value;
                if (dto.CriteriaJson != null) achievement.CriteriaJson = dto.CriteriaJson;
                if (dto.IsActive.HasValue) achievement.IsActive = dto.IsActive.Value;
                achievement.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.Achievements.UpdateAsync(achievement);
                await _unitOfWork.SaveChangesAsync();

                var result = await _unitOfWork.Achievements.GetByIdAsync(id);
                var achievementDto = _mapper.Map<AchievementDto>(result!);

                return Result<AchievementDto>.Success(achievementDto);
            }
            catch (Exception ex)
            {
                return Result<AchievementDto>.Failure($"Failed to update achievement: {ex.Message}");
            }
        }

        public async Task<Result<bool>> DeleteAchievementAsync(Guid id)
        {
            try
            {
                var achievement = await _unitOfWork.Achievements.GetByIdAsync(id);
                if (achievement == null)
                {
                    return Result<bool>.Failure("Achievement not found");
                }

                await _unitOfWork.Achievements.DeleteAsync(achievement);
                await _unitOfWork.SaveChangesAsync();

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure($"Failed to delete achievement: {ex.Message}");
            }
        }

        public async Task<Result<bool>> ToggleAchievementStatusAsync(Guid id, bool isActive)
        {
            try
            {
                var achievement = await _unitOfWork.Achievements.GetByIdAsync(id);
                if (achievement == null)
                {
                    return Result<bool>.Failure("Achievement not found");
                }

                achievement.IsActive = isActive;
                achievement.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.Achievements.UpdateAsync(achievement);
                await _unitOfWork.SaveChangesAsync();

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure($"Failed to toggle achievement status: {ex.Message}");
            }
        }
    }
}

