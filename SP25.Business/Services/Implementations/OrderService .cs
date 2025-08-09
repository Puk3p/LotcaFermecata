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
using SP25.Domain.Repository;

namespace SP25.Business.Services.Implementations
{
    public class OrderService : IOrderService
    {
        private readonly MyDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly IRepository<Order> _repository;

        public OrderService(MyDbContext dbContext, IMapper mapper, IRepository<Order> repository)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _repository = repository;
        }

        public async Task<OrderDto> CreateOrderAsync(string userId, CreateOrderDto dto)
        {
            Console.WriteLine($"[DEBUG] UserID: {userId ?? "ANON"}");
            Console.WriteLine($"[DEBUG] ClientName: {dto.ClientName}");
            Console.WriteLine($"[DEBUG] ClientPhone: {dto.ClientPhone}");
            Console.WriteLine($"[DEBUG] TargetZone: {dto.TargetZone}");
            Console.WriteLine($"[DEBUG] Items count: {dto.Items?.Count}");

            if (!Enum.TryParse<WorkZone>(dto.TargetZone, true, out var targetZone))
            {
                Console.WriteLine($"[ERROR] TargetZone invalid: {dto.TargetZone}");
                throw new ArgumentException($"Invalid target zone: {dto.TargetZone}");
            }

            var order = new Order
            {
                Id = Guid.NewGuid(),
                PlacedByUserId = userId ?? "anonymous",
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

            try
            {
                _dbContext.Orders.Add(order);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("[ERROR] Exception during SaveChangesAsync");
                Console.WriteLine(ex.ToString());
                throw;
            }

            return _mapper.Map<OrderDto>(order);
        }




        public async Task<List<OrderDto>> GetOrdersForZoneAsync(string zone)
        {
            if (!Enum.TryParse<WorkZone>(zone, true, out var targetZone))
                throw new ArgumentException("Invalid zone");

            var orders = await _dbContext.Orders
                .Where(o => o.TargetZone == targetZone)
                .Include(o => o.Items)
                .OrderBy(o => o.Status == OrderStatus.Pending ? 0 :
                              o.Status == OrderStatus.InProgress ? 1 :
                              o.Status == OrderStatus.Done ? 2 : 3)
                .ThenBy(o => o.PlacedAt)
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

        public async Task<OrderDto> GetOrderByIdAsync(Guid orderId)
        {
            var order = await _dbContext.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                return null;

            return _mapper.Map<OrderDto>(order);
        }

        public async Task<bool> UpdateOrderAsync(Guid orderId, UpdateOrderDto dto)
        {
            var order = await _dbContext.Orders.Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                return false;

            order.ClientName = dto.ClientName;
            order.ClientPhone = dto.ClientPhone;

            if (Enum.TryParse<WorkZone>(dto.TargetZone, true, out var zone))
                order.TargetZone = zone;

            order.Items.Clear();
            foreach (var itemDto in dto.Items)
            {
                order.Items.Add(new OrderItem
                {
                    ProductName = itemDto.ProductName,
                    Quantity = itemDto.Quantity
                });
            }

            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<OrderDto>> GetAllAsync()
        {
            var list = await _dbContext.Orders
                .Include(o => o.Items)
                .OrderByDescending(o => o.PlacedAt)
                .ToListAsync();

            return _mapper.Map<IEnumerable<OrderDto>>(list);
        }

        public async Task<GroupedOrdersDto> GetGroupedByDateAsync()
        {
            var allOrders = await _repository.GetAllAsync();
            var now = DateTime.UtcNow.AddHours(3);
            var todayString = now.ToString("dd.MM.yyyy");

            var grouped = new GroupedOrdersDto();

            foreach (var order in allOrders)
            {
                var date = order.PlacedAt.AddHours(3).ToString("dd.MM.yyyy");

                if (date == todayString)
                {
                    switch (order.Status)
                    {
                        case OrderStatus.Pending:
                        case OrderStatus.InProgress:
                            grouped.Active.Add(_mapper.Map<OrderDto>(order));
                            break;
                        case OrderStatus.Done:
                            grouped.Completed.Add(_mapper.Map<OrderDto>(order));
                            break;
                        case OrderStatus.Cancelled:
                            grouped.Cancelled.Add(_mapper.Map<OrderDto>(order));
                            break;
                    }
                }
                else
                {
                    if (!grouped.ArchivedGrouped.ContainsKey(date))
                    {
                        grouped.ArchivedGrouped[date] = new List<OrderDto>();
                    }

                    grouped.ArchivedGrouped[date].Add(_mapper.Map<OrderDto>(order));
                }
            }
            return grouped;
        }
    }
}
