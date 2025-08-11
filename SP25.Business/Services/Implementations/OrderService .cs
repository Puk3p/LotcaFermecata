using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SP25.Business.ModelDTOs;
using SP25.Business.Services.Contracts;
using SP25.Domain.Context;
using SP25.Domain.Models;
using SP25.Domain.Repository;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP25.Business.Services.Implementations
{
    public class OrderService : IOrderService
    {
        private readonly MyDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly IRepository<Order> _repository;
        private readonly IPriceCatalog _prices;

        public OrderService(MyDbContext dbContext, IMapper mapper, IRepository<Order> repository,
            IPriceCatalog prices)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _repository = repository;
            _prices = prices;
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



        public async Task<bool> UpdateStatusAsync(UpdateOrderStatusDto dto)
        {
            if (dto == null || dto.OrderId == Guid.Empty || string.IsNullOrWhiteSpace(dto.NewStatus))
                return false;

            var order = await _dbContext.Orders.FindAsync(dto.OrderId);
            if (order == null)
                return false;

            if (!Enum.TryParse<OrderStatus>(dto.NewStatus, true, out var status))
                return false;

            order.Status = status;
            await _dbContext.SaveChangesAsync();
            return true;
        }


        public async Task<Dictionary<string, DayArchiveDto>> GetArchivedGroupedDetailedAsync(
    bool paidOnly = false, bool includePayments = false)
        {
            var roTz = TimeZoneInfo.FindSystemTimeZoneById("E. Europe Standard Time");

            var all = await _dbContext.Orders
                .Include(o => o.Items)
                .ToListAsync();

            // Arhivate = Done sau Cancelled (poți ajusta)
            var archived = all
                .Where(o => o.Status == OrderStatus.Done || o.Status == OrderStatus.Cancelled)
                .ToList();

            string DayKey(DateTime utc)
            {
                var local = utc.Kind == DateTimeKind.Utc
                    ? TimeZoneInfo.ConvertTimeFromUtc(utc, roTz)
                    : utc;
                return local.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            }

            var result = new Dictionary<string, DayArchiveDto>();

            foreach (var grp in archived.GroupBy(o => DayKey(o.PlacedAt)))
            {
                var dayOrders = grp.ToList();

                // Agregăm pe produse doar din Done; dacă paidOnly==true, filtrăm și după “paid”
                var sourceForSummary = dayOrders
                    .Where(o => o.Status == OrderStatus.Done)
                    .Where(o => !paidOnly || IsPaidOrder(o))
                    .ToList();

                // map produs -> (set comenzi, qty, unit, total)
                var map = new Dictionary<string, (HashSet<Guid> orderIds, int qty, decimal? unit, decimal? total)>();

                foreach (var o in sourceForSummary)
                {
                    foreach (var it in (o.Items ?? new List<OrderItem>()))
                    {
                        var name = (it.ProductName ?? "Produs").Trim();
                        var qty = it.Quantity;
                        var unit = _prices.GetPriceOrNull(it.ProductName);

                        if (!map.TryGetValue(name, out var agg))
                            map[name] = (new HashSet<Guid>(), 0, unit, unit.HasValue ? 0m : (decimal?)null);

                        agg = map[name];
                        agg.orderIds.Add(o.Id);
                        agg.qty += qty;

                        if (unit.HasValue)
                        {
                            agg.total ??= 0m;
                            agg.total += unit.Value * qty;
                            if (!agg.unit.HasValue) agg.unit = unit.Value;
                        }

                        map[name] = agg;
                    }
                }

                var lines = map
                    .Select(kv => new ProductSummaryDto
                    {
                        ProductName = kv.Key,
                        OrdersCount = kv.Value.orderIds.Count,
                        Quantity = kv.Value.qty,
                        UnitPrice = kv.Value.unit,
                        TotalPaid = kv.Value.total
                    })
                    .OrderByDescending(l => l.Quantity)
                    .ThenBy(l => l.ProductName)
                    .ToList();

                decimal? revenue = lines.Any(l => l.TotalPaid.HasValue)
                    ? lines.Sum(l => l.TotalPaid ?? 0m)
                    : (decimal?)null;

                var totals = new DayTotalsDto
                {
                    // dacă paidOnly=false, acestea devin „OrdersDone/ItemsDone”, dar păstrăm denumirea pentru compatibilitate
                    OrdersPaid = sourceForSummary.Count,
                    ItemsPaid = lines.Sum(l => l.Quantity),
                    Revenue = revenue
                };

                if (includePayments)
                    totals.PaymentMethods = CountPaymentMethods(sourceForSummary);

                result[grp.Key] = new DayArchiveDto
                {
                    Orders = dayOrders.Select(o => _mapper.Map<OrderDto>(o)).ToList(),
                    Summary = lines,
                    Totals = totals
                };
            }

            return result
                .OrderByDescending(kv => kv.Key)
                .ToDictionary(kv => kv.Key, kv => kv.Value);
        }

        // --- helpers (nemodificate) ---
        private static bool IsPaidOrder(Order o)
        {
            var psProp = o.GetType().GetProperty("PaymentStatus")?.GetValue(o)?.ToString()?.Trim().ToLowerInvariant();
            if (psProp is "paid" or "plătit" or "platit") return true;
            var isPaidProp = o.GetType().GetProperty("IsPaid")?.GetValue(o) as bool?;
            if (isPaidProp == true) return true;
            return false;
        }

        private static decimal? TryGetUnitPrice(OrderItem it)
        {
            var p = it.GetType().GetProperty("UnitPrice")?.GetValue(it)
                 ?? it.GetType().GetProperty("Price")?.GetValue(it);
            return p switch
            {
                decimal d => d,
                double d => (decimal)d,
                float f => (decimal)f,
                int i => i,
                long l => l,
                _ => null
            };
        }

        private static Dictionary<string, int> CountPaymentMethods(IEnumerable<Order> orders)
        {
            var dict = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var o in orders)
            {
                var m = o.GetType().GetProperty("PaymentMethod")?.GetValue(o)?.ToString() ?? "Necunoscut";
                dict[m] = dict.TryGetValue(m, out var c) ? c + 1 : 1;
            }
            return dict;
        }
    }
}