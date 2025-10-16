using FStreak.Application.DTOs;
using FStreak.Application.Services.Interface;
using Microsoft.Extensions.Configuration;
using System;
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
                // Get App ID and App Certificate from configuration
                string appId = _configuration["Agora:AppId"];
                string appCertificate = _configuration["Agora:AppCertificate"];

                if (string.IsNullOrEmpty(appId) || string.IsNullOrEmpty(appCertificate))
                {
                    throw new InvalidOperationException("Agora App ID or App Certificate is not configured.");
                }

                // Use official RtcTokenBuilder2 (AccessToken2) - Format 007
                uint uid = 0; // Use 0 for universal token, or parse userId if needed
                uint expirationTimeInSeconds = 3600; // 1 hour

                // Use RolePublisher for full RTC privileges
                var token = AgoraIO.Media.RtcTokenBuilder2.buildTokenWithUid(
                    appId,
                    appCertificate,
                    channelName,
                    uid,
                    AgoraIO.Media.RtcTokenBuilder2.Role.RolePublisher,
                    expirationTimeInSeconds,
                    expirationTimeInSeconds
                );

                return await Task.FromResult(Result<AgoraTokenResponse>.Success(new AgoraTokenResponse
                {
                    Token = token,
                    AppId = appId,
                    ChannelName = channelName,
                    Uid = uid.ToString(),
                    Expiration = DateTime.UtcNow.AddSeconds(expirationTimeInSeconds)
                }));
            }
            catch (Exception ex)
            {
                return Result<AgoraTokenResponse>.Failure($"Failed to generate token: {ex.Message}");
            }
        }
    }
}
