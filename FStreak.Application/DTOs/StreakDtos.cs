using System.Text.Json.Serialization;

namespace FStreak.Application.DTOs
{
    public class StreakInfoDto
    {
        public string UserId { get; set; } = string.Empty;
        public int CurrentStreak { get; set; }
        public int LongestStreak { get; set; }
        public DateTime? LastCheckInDate { get; set; }
        public string TimeZone { get; set; } = string.Empty;
        public List<DateTime> StreakHistory { get; set; } = new();
    }

    public class StreakCheckInDto
    {
        public DateTime Date { get; set; }
        
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public StreakSource Source { get; set; }
    }

    public class StreakLeaderboardEntryDto
    {
        public string UserId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public int CurrentStreak { get; set; }
    }

    public class StreakLeaderboardDto
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public LeaderboardPeriod Period { get; set; }
        public List<StreakLeaderboardEntryDto> Items { get; set; } = new();
    }

    public class LeaderboardRequestDto
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public LeaderboardScope Scope { get; set; } = LeaderboardScope.Global;
        
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public LeaderboardPeriod Period { get; set; } = LeaderboardPeriod.Week;
        
        public int? GroupId { get; set; }
    }

    public enum StreakSource
    {
        Manual,
        Auto,
        GroupSession,
        PhotoCheckIn
    }

    public enum LeaderboardPeriod
    {
        Week,
        Month
    }

    public enum LeaderboardScope
    {
        Global,
        Group
    }
}