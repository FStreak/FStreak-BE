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

        // Group Study related DbSets
        public DbSet<GroupMember> GroupMembers { get; set; }
        public DbSet<GroupInvite> GroupInvites { get; set; }
        public DbSet<SessionParticipant> SessionParticipants { get; set; }
        public DbSet<SessionMessage> SessionMessages { get; set; }
        public DbSet<SessionReaction> SessionReactions { get; set; }

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

            modelBuilder.Entity<PomodoroConfig>()
                .HasNoKey();
        }
    }
}