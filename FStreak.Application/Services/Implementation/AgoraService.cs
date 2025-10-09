using FStreak.Application.DTOs;
using FStreak.Application.Services.Interface;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FStreak.Application.Services.Implementation
{
    public class AgoraService : IAgoraService
    {
        private readonly IConfiguration _configuration;

        public AgoraService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<Result<AgoraTokenResponse>> GenerateTokenAsync(string channelName, string userId)
        {
            try
            {
                // Lấy App ID và App Certificate từ cấu hình
                string appId = _configuration["Agora:AppId"];
                string appCertificate = _configuration["Agora:AppCertificate"];
                // Kiểm tra nếu App ID hoặc App Certificate không được cấu hình
                if (string.IsNullOrEmpty(appId) || string.IsNullOrEmpty(appCertificate))
                {
                    throw new InvalidOperationException("Agora App ID or App Certificate is not configured.");
                }

                var token = GenerateRtcToken(appId, appCertificate, channelName, userId, 3600);

                return Result<AgoraTokenResponse>.Success(new AgoraTokenResponse
                {
                    Token = token,
                    AppId = appId,
                    ChannelName = channelName,
                    Uid = userId,
                    Expiration = DateTime.UtcNow.AddHours(1)
                });
            }
            catch (Exception ex)
            {
                return Result<AgoraTokenResponse>.Failure($"Failed to generate token: {ex.Message}");
            }

        }

        private string GenerateRtcToken(string appId, string appCertificate, string channelName, string userId, uint expireTimeInSeconds)
        {
            // Thời gian hiện tại tính bằng giây
            uint currentTimestamp = (uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            uint expireTimestamp = currentTimestamp + expireTimeInSeconds;

            // Build token với AccessToken và các đặc quyền
            Dictionary<string, uint> services = new Dictionary<string, uint>();

            // Các đặc quyền
            // Kết nối đến kênh
            services.Add("service", currentTimestamp);

            // Tổng hợp tất cả các quyền bằng phép OR bit
            uint privileges = 0;

            // Quyền tham gia kênh
            privileges |= (1 << 0);  // Tương đương với Privileges.KJoinChannel

            // Quyền công bố video
            privileges |= (1 << 1);  // Tương đương với Privileges.KPublishVideoStream

            // Quyền công bố âm thanh
            privileges |= (1 << 2);  // Tương đương với Privileges.KPublishAudioStream

            // Thêm tổng hợp các quyền vào Dictionary một lần duy nhất
            services.Add("privilege", privileges);

            // Tạo token sử dụng các thông số đã chuẩn bị
            string signature = GenerateSignature(appId, appCertificate, channelName, userId, services, expireTimestamp);

            // Tạo token cuối cùng
            string token = $"{signature}:{expireTimestamp}:{channelName}:{userId}";

            return Convert.ToBase64String(Encoding.UTF8.GetBytes(token));
        }

        private string GenerateSignature(string appId, string appCertificate, string channelName, string uid, Dictionary<string, uint> services, uint expireTimestamp)
        {
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(appCertificate)))
            {
                // Tạo chuỗi cần hash theo đúng định dạng của Agora
                string message = $"{appId}{channelName}{uid}";

                // Thêm thông tin về services và expireTimestamp
                foreach (var service in services)
                {
                    message += $"{service.Key}{service.Value}";
                }
                message += expireTimestamp.ToString();

                // Tạo signature
                byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
                return Convert.ToHexString(hash).ToLower();
            }
        }
    }
}
