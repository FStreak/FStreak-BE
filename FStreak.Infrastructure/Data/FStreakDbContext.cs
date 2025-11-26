using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using FStreak.Domain.Entities;

namespace FStreak.Infrastructure.Data
{
    public class FStreakDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        public FStreakDbContext(DbContextOptions<FStreakDbContext> options)
            : base(options)
        {
        }

        public DbSet<StreakLog> StreakLogs { get; set; }
        public DbSet<StudyGroup> StudyGroups { get; set; }
        public DbSet<StudySession> StudySessions { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Challenge> Challenges { get; set; }
        public DbSet<UserChallenge> UserChallenges { get; set; }
        public DbSet<Badge> Badges { get; set; }
        public DbSet<StudyRoom> StudyRooms { get; set; }
        public DbSet<RoomUser> RoomUsers { get; set; }
        public DbSet<RoomMessage> RoomMessages { get; set; }
        public DbSet<UserBadge> UserBadges { get; set; }
        public DbSet<StudyWallPost> StudyWallPosts { get; set; }
        public DbSet<UserFriend> UserFriends { get; set; }
        public DbSet<Reaction> Reactions { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Reminder> Reminders { get; set; }
        public DbSet<PushSubscription> PushSubscriptions { get; set; }
        public DbSet<UserStreakHistory> UserStreakHistories { get; set; }
        public DbSet<Payment> Payments { get; set; }

        // Group Study related DbSets
        public DbSet<GroupMember> GroupMembers { get; set; }
        public DbSet<GroupInvite> GroupInvites { get; set; }
        public DbSet<SessionParticipant> SessionParticipants { get; set; }
        public DbSet<SessionMessage> SessionMessages { get; set; }
        public DbSet<SessionReaction> SessionReactions { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<Achievement> Achievements { get; set; }
        public DbSet<UserAchievement> UserAchievements { get; set; }
        public DbSet<ShopItem> ShopItems { get; set; }
        public DbSet<ShopOrder> ShopOrders { get; set; }
        public DbSet<ShopOrderItem> ShopOrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure ApplicationUser entity
            modelBuilder.Entity<ApplicationUser>()
                .HasMany(u => u.StreakLogs)
                .WithOne(s => s.User)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure StudySession relationships
            modelBuilder.Entity<StudySession>()
                .HasOne(s => s.Group)
                .WithMany()
                .HasForeignKey(s => s.GroupId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure PomodoroConfig as an owned type of StudySession
            modelBuilder.Entity<StudySession>()
                .OwnsOne(s => s.PomodoroConfig, ownedBuilder =>
                {
                    // Use default column naming; configure if needed
                });

            // Configure UserChallenge relationships
            modelBuilder.Entity<UserChallenge>()
                .HasOne(uc => uc.User)
                .WithMany(u => u.UserChallenges)
                .HasForeignKey(uc => uc.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserChallenge>()
                .HasOne(uc => uc.Challenge)
                .WithMany()
                .HasForeignKey(uc => uc.ChallengeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure UserBadge relationships
            modelBuilder.Entity<UserBadge>()
                .HasOne(ub => ub.User)
                .WithMany(u => u.UserBadges)
                .HasForeignKey(ub => ub.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserBadge>()
                .HasOne(ub => ub.Badge)
                .WithMany()
                .HasForeignKey(ub => ub.BadgeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure UserFriend relationships
            modelBuilder.Entity<UserFriend>()
                .HasOne(uf => uf.User)
                .WithMany(u => u.Friends)
                .HasForeignKey(uf => uf.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Reminder relationships
            modelBuilder.Entity<Reminder>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserFriend>()
                .HasOne(uf => uf.Friend)
                .WithMany(u => u.FriendOf)
                .HasForeignKey(uf => uf.FriendId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Reaction relationships
            modelBuilder.Entity<Reaction>()
                .HasOne(r => r.Sender)
                .WithMany(u => u.SentReactions)
                .HasForeignKey(r => r.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Reaction>()
                .HasOne(r => r.Recipient)
                .WithMany(u => u.ReceivedReactions)
                .HasForeignKey(r => r.RecipientId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure StudyWallPost relationships
            modelBuilder.Entity<StudyWallPost>()
                .HasOne(p => p.User)
                .WithMany(u => u.StudyWallPosts)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure PushSubscription relationships
            modelBuilder.Entity<PushSubscription>()
                .HasOne(ps => ps.User)
                .WithMany()
                .HasForeignKey(ps => ps.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure RefreshToken relationships
            modelBuilder.Entity<RefreshToken>()
                .HasOne(rt => rt.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure StudyRoom relationships
            modelBuilder.Entity<StudyRoom>()
                .HasOne(sr => sr.CreatedBy)
                .WithMany()
                .HasForeignKey(sr => sr.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure UserStreakHistory relationships
            modelBuilder.Entity<UserStreakHistory>()
                .HasOne(ush => ush.User)
                .WithMany()
                .HasForeignKey(ush => ush.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserStreakHistory>()
                .HasOne(ush => ush.StudyRoom)
                .WithMany()
                .HasForeignKey(ush => ush.StudyRoomId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure unique constraints
            modelBuilder.Entity<Subject>()
                .HasIndex(s => s.Code)
                .IsUnique();

            // Configure GroupMember relationships
            modelBuilder.Entity<GroupMember>()
                .HasOne(gm => gm.User)
                .WithMany(u => u.GroupMemberships)
                .HasForeignKey(gm => gm.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GroupMember>()
                .HasOne(gm => gm.Group)
                .WithMany(g => g.Members)
                .HasForeignKey(gm => gm.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure GroupInvite relationships
            modelBuilder.Entity<GroupInvite>()
                .HasOne(gi => gi.InvitedBy)
                .WithMany(u => u.SentGroupInvites)
                .HasForeignKey(gi => gi.InvitedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<GroupInvite>()
                .HasOne(gi => gi.InvitedUser)
                .WithMany(u => u.ReceivedGroupInvites)
                .HasForeignKey(gi => gi.InvitedUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<GroupInvite>()
                .HasOne(gi => gi.Group)
                .WithMany(g => g.Invites)
                .HasForeignKey(gi => gi.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure SessionParticipant relationships
            modelBuilder.Entity<SessionParticipant>()
                .HasOne(sp => sp.User)
                .WithMany(u => u.SessionParticipations)
                .HasForeignKey(sp => sp.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SessionParticipant>()
                .HasOne(sp => sp.Session)
                .WithMany(s => s.Participants)
                .HasForeignKey(sp => sp.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure SessionMessage relationships
            modelBuilder.Entity<SessionMessage>()
                .HasOne(sm => sm.User)
                .WithMany(u => u.SessionMessages)
                .HasForeignKey(sm => sm.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SessionMessage>()
                .HasOne(sm => sm.Session)
                .WithMany(s => s.Messages)
                .HasForeignKey(sm => sm.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure SessionReaction relationships
            modelBuilder.Entity<SessionReaction>()
                .HasOne(sr => sr.User)
                .WithMany(u => u.SessionReactions)
                .HasForeignKey(sr => sr.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SessionReaction>()
                .HasOne(sr => sr.Message)
                .WithMany(m => m.Reactions)
                .HasForeignKey(sr => sr.MessageId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Lesson relationships
            modelBuilder.Entity<Lesson>()
                .HasOne(l => l.CreatedBy)
                .WithMany()
                .HasForeignKey(l => l.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Achievement relationships
            modelBuilder.Entity<Achievement>()
                .HasIndex(a => a.Code)
                .IsUnique();

            modelBuilder.Entity<UserAchievement>()
                .HasOne(ua => ua.User)
                .WithMany(u => u.UserAchievements)
                .HasForeignKey(ua => ua.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserAchievement>()
                .HasOne(ua => ua.Achievement)
                .WithMany(a => a.UserAchievements)
                .HasForeignKey(ua => ua.AchievementId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure unique constraint: User can only have one instance of each achievement
            modelBuilder.Entity<UserAchievement>()
                .HasIndex(ua => new { ua.UserId, ua.AchievementId })
                .IsUnique();

            // Configure ShopItem relationships
            modelBuilder.Entity<ShopItem>()
                .HasIndex(si => si.Code)
                .IsUnique();

            // Configure ShopOrder relationships
            modelBuilder.Entity<ShopOrder>()
                .HasOne(so => so.User)
                .WithMany()
                .HasForeignKey(so => so.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ShopOrder>()
                .HasIndex(so => so.OrderNumber)
                .IsUnique();

            // Configure ShopOrderItem relationships
            modelBuilder.Entity<ShopOrderItem>()
                .HasOne(soi => soi.Order)
                .WithMany(so => so.OrderItems)
                .HasForeignKey(soi => soi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ShopOrderItem>()
                .HasOne(soi => soi.ShopItem)
                .WithMany(si => si.OrderItems)
                .HasForeignKey(soi => soi.ShopItemId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}