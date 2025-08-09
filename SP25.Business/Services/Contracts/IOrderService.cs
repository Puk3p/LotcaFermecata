using SP25.Business.ModelDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP25.Business.Services.Contracts
{
    public interface IOrderService
    {
        Task<OrderDto> CreateOrderAsync(string userId, CreateOrderDto dto);
        Task<List<OrderDto>> GetOrdersForZoneAsync(string zone);
        Task<bool> UpdateOrderStatusAsync(Guid orderId, string newStatus);
        Task<OrderDto> GetOrderByIdAsync(Guid orderId);
        Task<bool> UpdateOrderAsync(Guid orderId, UpdateOrderDto dto);
        Task<IEnumerable<OrderDto>> GetAllAsync();
        Task<List<OrderDto>> GetActiveOrdersAsync();
        Task<List<OrderDto>> GetCompletedOrdersAsync();
        Task<List<OrderDto>> GetCancelledOrdersAsync();
        Task<Dictionary<string, List<OrderDto>>> GetArchivedGroupedAsync();
        Task<bool> UpdateStatusAsync(UpdateOrderStatusDto dto);

    }
}