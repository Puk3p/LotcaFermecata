using AutoMapper;
using SP25.Business.ModelDTOs;
using SP25.Business.Services.Contracts;
using SP25.Domain.Context;
using SP25.Domain.Models;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP25.Business.Services.Implementations
{
    public class OrderService : IOrderService
    {
        private readonly MyDbContext _dbContext;
        private readonly IMapper _mapper;

        public OrderService(MyDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<OrderDto> CreateOrderAsync(string userId, CreateOrderDto dto)
        {
            if (!Enum.TryParse<WorkZone>(dto.TargetZone, true, out var targetZone))
                throw new ArgumentException("Invalid target zone");

            var order = new Order
            {
                Id = Guid.NewGuid(),
                PlacedByUserId = userId,
                ClientName = dto.ClientName,
                ClientPhone = dto.ClientPhone,
                TargetZone = targetZone,
                PlacedAt = DateTime.UtcNow,
                Status = OrderStatus.Pending,
                Items = dto.Items?.Select(i => new OrderItem
                {
                    ProductName = i.ProductName,
                    Quantity = i.Quantity
                }).ToList() ?? new List<OrderItem>()
            };

            _dbContext.Orders.Add(order);
            await _dbContext.SaveChangesAsync();

            return _mapper.Map<OrderDto>(order);
        }


        public async Task<List<OrderDto>> GetOrdersForZoneAsync(string zone)
        {
            if (!Enum.TryParse<WorkZone>(zone, true, out var targetZone))
                throw new ArgumentException("Invalid zone");

            var orders = await _dbContext.Orders
                .Where(o => o.TargetZone == targetZone)
                .Include(o => o.Items)
                .OrderByDescending(o => o.PlacedAt)
                .ToListAsync();

            return _mapper.Map<List<OrderDto>>(orders);
        }

        public async Task<bool> UpdateOrderStatusAsync(Guid orderId, string newStatus)
        {
            var order = await _dbContext.Orders.FindAsync(orderId);
            if (order == null)
                return false;

            if (!Enum.TryParse<OrderStatus>(newStatus, true, out var status))
                return false;

            order.Status = status;
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}
