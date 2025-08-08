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

        public async Task<List<OrderDto>> GetActiveOrdersAsync()
        {
            var romaniaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("E. Europe Standard Time");
            var today = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, romaniaTimeZone).Date;

            var allOrders = await _dbContext.Orders
                .Include(o => o.Items)
                .ToListAsync();

            var filtered = allOrders
                .Where(o =>
                    TimeZoneInfo.ConvertTimeFromUtc(o.PlacedAt, romaniaTimeZone).Date == today &&
                    (o.Status == OrderStatus.Pending || o.Status == OrderStatus.InProgress))
                .OrderBy(o => o.PlacedAt)
                .ToList();

            return _mapper.Map<List<OrderDto>>(filtered);
        }

        public async Task<List<OrderDto>> GetCompletedOrdersAsync()
        {
            var romaniaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("E. Europe Standard Time");
            var today = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, romaniaTimeZone).Date;

            var allOrders = await _dbContext.Orders
                .Include(o => o.Items)
                .ToListAsync();

            var filtered = allOrders
                .Where(o =>
                    TimeZoneInfo.ConvertTimeFromUtc(o.PlacedAt, romaniaTimeZone).Date == today &&
                    o.Status == OrderStatus.Done)
                .OrderBy(o => o.PlacedAt)
                .ToList();

            return _mapper.Map<List<OrderDto>>(filtered);
        }

        public async Task<List<OrderDto>> GetCancelledOrdersAsync()
        {
            var romaniaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("E. Europe Standard Time");
            var today = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, romaniaTimeZone).Date;

            var allOrders = await _dbContext.Orders
                .Include(o => o.Items)
                .ToListAsync();

            var filtered = allOrders
                .Where(o =>
                    TimeZoneInfo.ConvertTimeFromUtc(o.PlacedAt, romaniaTimeZone).Date == today &&
                    o.Status == OrderStatus.Cancelled)
                .OrderBy(o => o.PlacedAt)
                .ToList();

            return _mapper.Map<List<OrderDto>>(filtered);
        }

        public async Task<Dictionary<string, List<OrderDto>>> GetArchivedGroupedAsync()
        {
            // 1. Fusul orar România
            var romaniaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("E. Europe Standard Time");

            // 2. Ziua curentă în România (la nivel de dată, fără oră)
            var today = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, romaniaTimeZone).Date;

            // 3. Obține toate comenzile în memorie (client-side), apoi filtrează după dată
            var allOrders = await _dbContext.Orders
                .Include(o => o.Items)
                .ToListAsync(); // => executat pe SQL Server, deci fără TimeZone conversion aici

            // 4. Filtrare și grupare locală
            var grouped = allOrders
                .Where(o => TimeZoneInfo.ConvertTimeFromUtc(o.PlacedAt, romaniaTimeZone).Date < today)
                .GroupBy(o => TimeZoneInfo.ConvertTimeFromUtc(o.PlacedAt, romaniaTimeZone).ToString("dd.MM.yyyy"))
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(o => _mapper.Map<OrderDto>(o)).ToList()
                );

            return grouped;
        }




    }
}
