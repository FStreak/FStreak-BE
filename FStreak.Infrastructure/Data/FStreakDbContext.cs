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
                .HasOne(s => s.StudyGroup)
                .WithMany()
                .HasForeignKey(s => s.StudyGroupId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StudySession>()
                .HasOne(s => s.User)
                .WithMany(u => u.StudySessions)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StudySession>()
                .HasOne(s => s.Subject)
                .WithMany()
                .HasForeignKey(s => s.SubjectId)
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
        }
    }
}