using System.Text.Json;
using FStreak.Application.DTOs;
using FStreak.Application.Services.Interface;
using FStreak.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WebPush;
using DbPushSubscription = FStreak.Domain.Entities.PushSubscription;

namespace FStreak.Application.Services.Implementation
{
    public class PushNotificationService : IPushNotificationService
    {
        private readonly ILogger<PushNotificationService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly WebPushClient _pushClient;
        private readonly VapidDetails _vapidDetails;

        public PushNotificationService(
            ILogger<PushNotificationService> logger,
            IUnitOfWork unitOfWork,
            IConfiguration configuration)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _pushClient = new WebPushClient();

            // Get VAPID details from configuration
            _vapidDetails = new VapidDetails(
                _configuration["WebPush:Subject"],
                _configuration["WebPush:PublicKey"],
                _configuration["WebPush:PrivateKey"]
            );
        }

        public async Task<Result<bool>> RegisterDeviceAsync(string userId, RegisterPushDeviceDto dto)
        {
            try
            {
                var exists = await _unitOfWork.PushSubscriptions.ExistsAsync(dto.Endpoint);
                if (exists)
                {
                    return Result<bool>.Failure("Subscription already exists");
                }

                var subscription = new DbPushSubscription
                {
                    UserId = userId,
                    Endpoint = dto.Endpoint,
                    P256dh = dto.Keys["p256dh"],
                    Auth = dto.Keys["auth"],
                    UserAgent = "Web Browser",
                    Enabled = true,
                    LastUsed = DateTime.UtcNow
                };

                await _unitOfWork.PushSubscriptions.AddAsync(subscription);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Registered push device for user {UserId}", userId);
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering push device for user {UserId}", userId);
                return Result<bool>.Failure("Failed to register push device");
            }
        }

        public async Task<Result<bool>> UnregisterDeviceAsync(string endpoint)
        {
            try
            {
                await _unitOfWork.PushSubscriptions.DisableSubscriptionAsync(endpoint);
                _logger.LogInformation("Unregistered push device with endpoint {Endpoint}", endpoint);
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unregistering push device with endpoint {Endpoint}", endpoint);
                return Result<bool>.Failure("Failed to unregister push device");
            }
        }

        public async Task<Result<bool>> SendNotificationAsync(string userId, PushNotificationDto notification)
        {
            try
            {
                var subscriptions = await _unitOfWork.PushSubscriptions.GetActiveSubscriptionsForUserAsync(userId);
                if (!subscriptions.Any())
                {
                    _logger.LogWarning("No active push subscriptions found for user {UserId}", userId);
                    return Result<bool>.Success(false);
                }

                var payload = JsonSerializer.Serialize(notification);
                var success = false;

                foreach (var subscription in subscriptions)
                {
                    try
                    {
                        var pushSubscription = new PushSubscription(
                            subscription.Endpoint,
                            subscription.P256dh,
                            subscription.Auth);

                        await _pushClient.SendNotificationAsync(pushSubscription, payload, _vapidDetails);

                        subscription.LastUsed = DateTime.UtcNow;
                        success = true;
                    }
                    catch (WebPushException ex)
                    {
                        if (ex.Message.Contains("410") || ex.Message.Contains("404"))
                        {
                            subscription.Enabled = false;
                            _logger.LogWarning("Subscription expired or not found: {Endpoint}", subscription.Endpoint);
                        }
                    }
                }

                await _unitOfWork.SaveChangesAsync();
                return Result<bool>.Success(success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending push notification to user {UserId}", userId);
                return Result<bool>.Failure("Failed to send push notification");
            }
        }

        public async Task<Result<bool>> SendNotificationToUsersAsync(IEnumerable<string> userIds, PushNotificationDto notification)
        {
            try
            {
                var uniqueUserIds = userIds.Distinct().ToList();
                var subscriptions = await _unitOfWork.PushSubscriptions.GetActiveSubscriptionsForUsersAsync(uniqueUserIds);

                if (!subscriptions.Any())
                {
                    _logger.LogWarning("No active push subscriptions found for users");
                    return Result<bool>.Success(false);
                }

                var payload = JsonSerializer.Serialize(notification);
                var success = false;

                foreach (var subscription in subscriptions)
                {
                    try
                    {
                        var pushSubscription = new PushSubscription(
                            subscription.Endpoint,
                            subscription.P256dh,
                            subscription.Auth);

                        await _pushClient.SendNotificationAsync(pushSubscription, payload, _vapidDetails);

                        subscription.LastUsed = DateTime.UtcNow;
                        success = true;
                    }
                    catch (WebPushException ex)
                    {
                        if (ex.Message.Contains("410") || ex.Message.Contains("404"))
                        {
                            subscription.Enabled = false;
                            _logger.LogWarning("Subscription expired or not found: {Endpoint}", subscription.Endpoint);
                        }
                    }
                }

                await _unitOfWork.SaveChangesAsync();
                return Result<bool>.Success(success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending push notifications to users");
                return Result<bool>.Failure("Failed to send push notifications");
            }
        }

        public async Task<Result<bool>> SendNotificationToSubscriptionAsync(string endpoint, PushNotificationDto notification)
        {
            try
            {
                var subscriptions = await _unitOfWork.PushSubscriptions
                                    .FindAsync(s => s.Endpoint == endpoint && s.Enabled);

                var subscription = subscriptions.FirstOrDefault();


                if (subscription == null)
                {
                    _logger.LogWarning("No active subscription found for endpoint {Endpoint}", endpoint);
                    return Result<bool>.Failure("Subscription not found or disabled");
                }

                var payload = JsonSerializer.Serialize(notification);
                var pushSubscription = new PushSubscription(
                    subscription.Endpoint,
                    subscription.P256dh,
                    subscription.Auth);

                await _pushClient.SendNotificationAsync(pushSubscription, payload, _vapidDetails);

                subscription.LastUsed = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync();

                return Result<bool>.Success(true);
            }
            catch (WebPushException ex)
            {
                if (ex.Message.Contains("410") || ex.Message.Contains("404"))
                {
                    await _unitOfWork.PushSubscriptions.DisableSubscriptionAsync(endpoint);
                    return Result<bool>.Failure("Subscription expired or not found");
                }
                _logger.LogError(ex, "Error sending push notification to endpoint {Endpoint}", endpoint);
                return Result<bool>.Failure("Failed to send push notification");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending push notification to endpoint {Endpoint}", endpoint);
                return Result<bool>.Failure("Failed to send push notification");
            }
        }
    }
}