namespace FStreak.Application.DTOs
{
    public class JoinRoomResponse
    {
        public RoomUserDto? RoomUser { get; set; }
        public AgoraTokenResponse? AgoraTokens { get; set; }
    }
}
