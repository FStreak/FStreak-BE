using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using FStreak.Application.DTOs;
using FStreak.Application.Services.Interface;
using FStreak.Domain.Entities;
using FStreak.Domain.Interfaces;
using FStreak.Domain.Common;

namespace FStreak.Application.Services.Implementation
{
    public class StudyGroupService : IStudyGroupService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<StudyGroupService> _logger;

        public StudyGroupService(IUnitOfWork unitOfWork, ILogger<StudyGroupService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // Helper methods
        private async Task<bool> IsGroupAdmin(int groupId, string userId)
        {
            // Check if user is the owner first
            var groups = await _unitOfWork.StudyGroups.FindAsync(g => g.StudyGroupId == groupId);
            var group = groups.FirstOrDefault();
            if (group == null)
                return false;
            
            if (group.OwnerId == userId)
                return true;

            // If not owner, check if they are an admin
            var member = await _unitOfWork.GroupMembers.FindAsync(m => 
                m.GroupId == groupId && 
                m.UserId == userId);
            return member.Any(m => m.Role == GroupRole.Admin);
        }

        private string GenerateInviteCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)])
                .ToArray());
        }

        private StudyGroupDto MapToDto(StudyGroup group, int memberCount = 0)
        {
            return new StudyGroupDto
            {
                Id = group.StudyGroupId,
                Name = group.Name,
                Description = group.Description,
                InviteCode = group.InviteCode,
                Visibility = group.Visibility,
                OwnerId = group.OwnerId,
                OwnerName = group.Owner?.UserName ?? "Unknown",
                MemberCount = memberCount,
                CreatedAt = group.CreatedAt,
                UpdatedAt = group.UpdatedAt
            };
        }

        private GroupMemberDto MapToMemberDto(GroupMember member)
        {
            return new GroupMemberDto
            {
                Id = member.GroupMemberId,
                GroupId = member.GroupId,
                UserId = member.UserId,
                UserName = member.User?.UserName ?? "Unknown",
                Role = member.Role,
                JoinedAt = member.JoinedAt,
                UpdatedAt = member.UpdatedAt
            };
        }

        private GroupInviteDto MapToInviteDto(GroupInvite invite)
        {
            return new GroupInviteDto
            {
                Id = invite.GroupInviteId,
                GroupId = invite.GroupId,
                GroupName = invite.Group?.Name ?? "Unknown",
                InvitedByUserId = invite.InvitedByUserId,
                InvitedByUserName = invite.InvitedBy?.UserName ?? "Unknown",
                InvitedUserId = invite.InvitedUserId,
                InvitedUserName = invite.InvitedUser?.UserName ?? "Unknown",
                Status = invite.Status,
                ExpiresAt = invite.ExpiresAt,
                CreatedAt = invite.CreatedAt,
                UpdatedAt = invite.UpdatedAt
            };
        }

        // Group Management Implementation
        public async Task<Result<StudyGroupDto>> CreateGroupAsync(string userId, CreateStudyGroupDto dto)
        {
            try
            {
                var group = new StudyGroup
                {
                    Name = dto.Name,
                    Description = dto.Description,
                    InviteCode = GenerateInviteCode(),
                    Visibility = dto.Visibility,
                    OwnerId = userId,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.StudyGroups.AddAsync(group);
                await _unitOfWork.SaveChangesAsync();

                // Add owner as a member with Admin role
                var member = new GroupMember
                {
                    GroupId = group.StudyGroupId,
                    UserId = userId,
                    Role = GroupRole.Admin,
                    JoinedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.GroupMembers.AddAsync(member);
                await _unitOfWork.SaveChangesAsync();

                return Result<StudyGroupDto>.Success(MapToDto(group, 1));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating study group");
                return Result<StudyGroupDto>.Failure("Failed to create study group");
            }
        }

        public async Task<Result<StudyGroupDto>> GetGroupByIdAsync(int groupId)
        {
            try
            {
                var groups = await _unitOfWork.StudyGroups.FindAsync(g => g.StudyGroupId == groupId);
                var group = groups.FirstOrDefault();
                
                if (group == null)
                    return Result<StudyGroupDto>.Failure("Group not found");

                var memberCount = await _unitOfWork.GroupMembers
                    .FindAsync(m => m.GroupId == groupId)
                    .ContinueWith(t => t.Result.Count());

                return Result<StudyGroupDto>.Success(MapToDto(group, memberCount));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting study group");
                return Result<StudyGroupDto>.Failure("Failed to get study group");
            }
        }

        public async Task<Result<List<StudyGroupDto>>> GetUserGroupsAsync(string userId)
        {
            try
            {
                var members = await _unitOfWork.GroupMembers.FindAsync(m => m.UserId == userId);
                var groups = new List<StudyGroupDto>();

                foreach (var member in members)
                {
                    var result = await GetGroupByIdAsync(member.GroupId);
                    if (result.Succeeded && result.Data != null)
                        groups.Add(result.Data);
                }

                return Result<List<StudyGroupDto>>.Success(groups);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user groups");
                return Result<List<StudyGroupDto>>.Failure("Failed to get user groups");
            }
        }

        public async Task<Result<GroupMemberDto>> AddMemberAsync(int groupId, string userId, bool isAdmin = false)
        {
            try
            {
                var existingMember = (await _unitOfWork.GroupMembers
                    .FindAsync(m => m.GroupId == groupId && m.UserId == userId))
                    .FirstOrDefault();

                if (existingMember != null)
                    return Result<GroupMemberDto>.Failure("User is already a member of this group");

                var member = new GroupMember
                {
                    GroupId = groupId,
                    UserId = userId,
                    Role = isAdmin ? GroupRole.Admin : GroupRole.Member,
                    JoinedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.GroupMembers.AddAsync(member);
                await _unitOfWork.SaveChangesAsync();

                return Result<GroupMemberDto>.Success(MapToMemberDto(member));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding group member");
                return Result<GroupMemberDto>.Failure("Failed to add member to group");
            }
        }

        public async Task<Result<List<GroupMemberDto>>> GetGroupMembersAsync(int groupId)
        {
            try
            {
                var members = await _unitOfWork.GroupMembers.FindAsync(m => m.GroupId == groupId);
                return Result<List<GroupMemberDto>>.Success(members.Select(MapToMemberDto).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting group members");
                return Result<List<GroupMemberDto>>.Failure("Failed to get group members");
            }
        }

        public async Task<Result<GroupInviteDto>> CreateInviteAsync(int groupId, string inviterId, CreateGroupInviteDto dto)
        {
            try
            {
                if (!await IsGroupAdmin(groupId, inviterId))
                    return Result<GroupInviteDto>.Failure("Only group admins can send invites");

                // Check if an invite already exists
                var existingInvite = await _unitOfWork.GroupInvites.FindAsync(i => 
                    i.GroupId == groupId && 
                    i.InvitedUserId == dto.InvitedUserId && 
                    i.Status == InviteStatus.Pending);

                if (existingInvite.Any())
                    return Result<GroupInviteDto>.Failure("An invite already exists for this user");

                var invite = new GroupInvite
                {
                    GroupId = groupId,
                    InvitedByUserId = inviterId,
                    InvitedUserId = dto.InvitedUserId,
                    Status = InviteStatus.Pending,
                    ExpiresAt = DateTime.UtcNow.AddDays(7), // Invites expire after 7 days
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.GroupInvites.AddAsync(invite);
                await _unitOfWork.SaveChangesAsync();

                return Result<GroupInviteDto>.Success(MapToInviteDto(invite));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating group invite");
                return Result<GroupInviteDto>.Failure("Failed to create group invite");
            }
        }

        public async Task<Result<GroupInviteDto>> RespondToInviteAsync(int inviteId, string userId, bool accept)
        {
            try
            {
                var invites = await _unitOfWork.GroupInvites.FindAsync(i => i.GroupInviteId == inviteId);
                var invite = invites.FirstOrDefault();

                if (invite == null)
                    return Result<GroupInviteDto>.Failure("Invite not found");

                if (invite.InvitedUserId != userId)
                    return Result<GroupInviteDto>.Failure("Only the invited user can respond to the invite");

                if (invite.Status != InviteStatus.Pending)
                    return Result<GroupInviteDto>.Failure("Invite is no longer pending");

                if (invite.ExpiresAt < DateTime.UtcNow)
                    return Result<GroupInviteDto>.Failure("Invite has expired");

                invite.Status = accept ? InviteStatus.Accepted : InviteStatus.Declined;
                invite.UpdatedAt = DateTime.UtcNow;

                if (accept)
                {
                    var addMemberResult = await AddMemberAsync(invite.GroupId, userId);
                    if (!addMemberResult.Succeeded)
                        return Result<GroupInviteDto>.Failure(addMemberResult.Error);
                }

                await _unitOfWork.SaveChangesAsync();

                return Result<GroupInviteDto>.Success(MapToInviteDto(invite));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error responding to group invite");
                return Result<GroupInviteDto>.Failure("Failed to respond to group invite");
            }
        }

        public async Task<Result<StudyGroupDto>> UpdateGroupAsync(int groupId, string userId, UpdateStudyGroupDto dto)
        {
            try
            {
                if (!await IsGroupAdmin(groupId, userId))
                    return Result<StudyGroupDto>.Failure("Only group admins can update the group");

                var groups = await _unitOfWork.StudyGroups.FindAsync(g => g.StudyGroupId == groupId);
                var group = groups.FirstOrDefault();
                if (group == null)
                    return Result<StudyGroupDto>.Failure("Group not found");

                if (dto.Name != null)
                    group.Name = dto.Name;
                if (dto.Description != null)
                    group.Description = dto.Description;
                if (dto.Visibility.HasValue)
                    group.Visibility = dto.Visibility.Value;

                group.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync();

                var memberCount = await _unitOfWork.GroupMembers
                    .FindAsync(m => m.GroupId == groupId)
                    .ContinueWith(t => t.Result.Count());

                return Result<StudyGroupDto>.Success(MapToDto(group, memberCount));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating study group");
                return Result<StudyGroupDto>.Failure("Failed to update study group");
            }
        }

        public async Task<Result<bool>> DeleteGroupAsync(int groupId, string userId)
        {
            try
            {
                var groups = await _unitOfWork.StudyGroups.FindAsync(g => g.StudyGroupId == groupId);
                var group = groups.FirstOrDefault();
                if (group == null)
                    return Result<bool>.Failure("Group not found");

                if (group.OwnerId != userId)
                    return Result<bool>.Failure("Only the group owner can delete the group");

                // Delete all related records
                var members = await _unitOfWork.GroupMembers.FindAsync(m => m.GroupId == groupId);
                foreach (var member in members)
                    await _unitOfWork.GroupMembers.DeleteAsync(member);

                var invites = await _unitOfWork.GroupInvites.FindAsync(i => i.GroupId == groupId);
                foreach (var invite in invites)
                    await _unitOfWork.GroupInvites.DeleteAsync(invite);

                await _unitOfWork.StudyGroups.DeleteAsync(group);
                await _unitOfWork.SaveChangesAsync();

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting study group");
                return Result<bool>.Failure("Failed to delete study group");
            }
        }

        public async Task<Result<List<StudyGroupDto>>> SearchGroupsAsync(string searchTerm)
        {
            try
            {
                var groups = await _unitOfWork.StudyGroups
                    .FindAsync(g => 
                        g.Visibility == GroupVisibility.Public &&
                        (g.Name.Contains(searchTerm) || g.Description.Contains(searchTerm)));

                var groupDtos = new List<StudyGroupDto>();
                foreach (var group in groups)
                {
                    var memberCount = await _unitOfWork.GroupMembers
                        .FindAsync(m => m.GroupId == group.StudyGroupId)
                        .ContinueWith(t => t.Result.Count());

                    groupDtos.Add(MapToDto(group, memberCount));
                }

                return Result<List<StudyGroupDto>>.Success(groupDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching study groups");
                return Result<List<StudyGroupDto>>.Failure("Failed to search study groups");
            }
        }

        public async Task<Result<bool>> RemoveMemberAsync(int groupId, string adminId, string userId)
        {
            try
            {
                if (!await IsGroupAdmin(groupId, adminId))
                    return Result<bool>.Failure("Only group admins can remove members");

                var members = await _unitOfWork.GroupMembers
                    .FindAsync(m => m.GroupId == groupId && m.UserId == userId);
                var member = members.FirstOrDefault();
                
                if (member == null)
                    return Result<bool>.Failure("Member not found");

                // Cannot remove the owner
                var group = (await _unitOfWork.StudyGroups.FindAsync(g => g.StudyGroupId == groupId)).FirstOrDefault();
                if (group?.OwnerId == userId)
                    return Result<bool>.Failure("Cannot remove the group owner");

                await _unitOfWork.GroupMembers.DeleteAsync(member);
                await _unitOfWork.SaveChangesAsync();

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing group member");
                return Result<bool>.Failure("Failed to remove member from group");
            }
        }

        public async Task<Result<bool>> LeaveGroupAsync(int groupId, string userId)
        {
            try
            {
                var group = (await _unitOfWork.StudyGroups.FindAsync(g => g.StudyGroupId == groupId)).FirstOrDefault();
                if (group == null)
                    return Result<bool>.Failure("Group not found");

                if (group.OwnerId == userId)
                    return Result<bool>.Failure("The group owner cannot leave the group");

                var members = await _unitOfWork.GroupMembers
                    .FindAsync(m => m.GroupId == groupId && m.UserId == userId);
                var member = members.FirstOrDefault();

                if (member == null)
                    return Result<bool>.Failure("Not a member of this group");

                await _unitOfWork.GroupMembers.DeleteAsync(member);
                await _unitOfWork.SaveChangesAsync();

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error leaving group");
                return Result<bool>.Failure("Failed to leave group");
            }
        }

        public async Task<Result<bool>> UpdateMemberRoleAsync(int groupId, string adminId, string userId, bool isAdmin)
        {
            try
            {
                if (!await IsGroupAdmin(groupId, adminId))
                    return Result<bool>.Failure("Only group admins can update member roles");

                var members = await _unitOfWork.GroupMembers
                    .FindAsync(m => m.GroupId == groupId && m.UserId == userId);
                var member = members.FirstOrDefault();

                if (member == null)
                    return Result<bool>.Failure("Member not found");

                // Cannot change the owner's role
                var group = (await _unitOfWork.StudyGroups.FindAsync(g => g.StudyGroupId == groupId)).FirstOrDefault();
                if (group?.OwnerId == userId)
                    return Result<bool>.Failure("Cannot change the group owner's role");

                member.Role = isAdmin ? GroupRole.Admin : GroupRole.Member;
                member.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync();

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating member role");
                return Result<bool>.Failure("Failed to update member role");
            }
        }

        public async Task<Result<GroupInviteDto>> GetInviteByIdAsync(int inviteId)
        {
            try
            {
                var invites = await _unitOfWork.GroupInvites
                    .FindAsync(i => i.GroupInviteId == inviteId);
                var invite = invites.FirstOrDefault();

                if (invite == null)
                    return Result<GroupInviteDto>.Failure("Invite not found");

                return Result<GroupInviteDto>.Success(MapToInviteDto(invite));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting group invite");
                return Result<GroupInviteDto>.Failure("Failed to get group invite");
            }
        }

        public async Task<Result<List<GroupInviteDto>>> GetPendingInvitesForUserAsync(string userId)
        {
            try
            {
                var invites = await _unitOfWork.GroupInvites
                    .FindAsync(i => 
                        i.InvitedUserId == userId && 
                        i.Status == InviteStatus.Pending &&
                        i.ExpiresAt > DateTime.UtcNow);

                return Result<List<GroupInviteDto>>.Success(invites.Select(MapToInviteDto).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending invites");
                return Result<List<GroupInviteDto>>.Failure("Failed to get pending invites");
            }
        }

        public async Task<Result<bool>> CancelInviteAsync(int inviteId, string inviterId)
        {
            try
            {
                var invites = await _unitOfWork.GroupInvites
                    .FindAsync(i => i.GroupInviteId == inviteId);
                var invite = invites.FirstOrDefault();

                if (invite == null)
                    return Result<bool>.Failure("Invite not found");

                if (invite.InvitedByUserId != inviterId)
                    return Result<bool>.Failure("Only the inviter can cancel the invite");

                if (invite.Status != InviteStatus.Pending)
                    return Result<bool>.Failure("Can only cancel pending invites");

                invite.Status = InviteStatus.Revoked;
                invite.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync();

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error canceling group invite");
                return Result<bool>.Failure("Failed to cancel group invite");
            }
        }
    }
}