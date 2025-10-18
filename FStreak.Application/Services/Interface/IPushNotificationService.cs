using FStreak.Application.DTOs;

namespace FStreak.Application.Services.Interface
{
    public interface IPushNotificationService
    {
        Task<Result<bool>> RegisterDeviceAsync(string userId, RegisterPushDeviceDto dto);
        Task<Result<bool>> UnregisterDeviceAsync(string endpoint);
        Task<Result<bool>> SendNotificationAsync(string userId, PushNotificationDto notification);
        Task<Result<bool>> SendNotificationToUsersAsync(IEnumerable<string> userIds, PushNotificationDto notification);
        Task<Result<bool>> SendNotificationToSubscriptionAsync(string endpoint, PushNotificationDto notification);
    }
}