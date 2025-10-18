using FStreak.Domain.Entities;

namespace FStreak.Domain.Interfaces
{
    public interface IPushSubscriptionRepository : IRepository<PushSubscription>
    {
        Task<List<PushSubscription>> GetActiveSubscriptionsForUserAsync(string userId);
        Task<List<PushSubscription>> GetActiveSubscriptionsForUsersAsync(IEnumerable<string> userIds);
        Task<bool> ExistsAsync(string endpoint);
        Task DisableSubscriptionAsync(string endpoint);
    }
}