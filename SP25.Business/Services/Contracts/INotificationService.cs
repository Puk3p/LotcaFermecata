using System.Threading.Tasks;
using SP25.Business.ModelDTOs;

namespace SP25.Business.Services.Contracts
{
    public interface INotificationService
    {
        Task SubscribeAsync(PushSubscribeDto dto);
        Task NotifyOrderStatusChangedAsync(string orderId, string newStatus, string? notifyRole);
        Task NotifyOrderStatusChangedAsync(string orderId, string newStatus);
    }
}
