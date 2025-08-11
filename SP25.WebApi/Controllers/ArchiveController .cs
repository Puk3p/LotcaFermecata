// SP25.WebApi.Controllers.ArchiveController.cs
using Microsoft.AspNetCore.Mvc;
using SP25.Business.ModelDTOs;
using SP25.Business.Services.Contracts;

[ApiController]
[Route("api/[controller]")]
public class ArchiveController : ControllerBase
{
    private readonly IOrderService _orderService;
    public ArchiveController(IOrderService orderService) => _orderService = orderService;

    [HttpGet("closeout")]
    public async Task<IActionResult> GetDailyCloseout([FromQuery] DateTime? date)
    {
        var roTz = TimeZoneInfo.FindSystemTimeZoneById("E. Europe Standard Time");
        var nowRo = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, roTz);
        var day = (date ?? nowRo.Date).Date;

        var orders = (await _orderService.GetAllAsync())
                     .Where(o => ToRo(o.PlacedAt, roTz).Date == day)
                     .ToList();

        var paid = orders.Where(o => IsCompleted(o.Status) && IsPaid(o)).ToList();

        var lines = paid
            .SelectMany(o => (o.Items ?? new List<OrderItemDto>())
                .Select(i => new { o.Id, i.ProductName, i.Quantity, UnitPrice = GetUnitPrice(i) }))
            .GroupBy(x => x.ProductName)
            .Select(g =>
            {
                var qty = g.Sum(x => x.Quantity);
                decimal? unit = g.Select(x => x.UnitPrice).Where(p => p.HasValue).Select(p => p!.Value).DefaultIfEmpty().FirstOrDefault();
                bool hasAnyPrice = g.Any(x => x.UnitPrice.HasValue);
                decimal? total = hasAnyPrice ? g.Sum(x => x.UnitPrice.GetValueOrDefault() * x.Quantity) : null;

                return new DailyCloseLineDto
                {
                    ProductName = g.Key,
                    OrdersCount = g.Select(x => x.Id).Distinct().Count(),
                    Quantity = qty,
                    UnitPrice = unit == 0 ? null : unit,
                    TotalPaid = total
                };
            })
            .OrderByDescending(l => l.Quantity)
            .ThenBy(l => l.ProductName)
            .ToList();

        // totaluri
        var totals = new DailyCloseTotalsDto
        {
            OrdersPaid = paid.Count,
            ItemsPaid = paid.SelectMany(o => o.Items ?? new()).Sum(i => i.Quantity),
            Revenue = lines.Any(l => l.TotalPaid.HasValue) ? lines.Sum(l => l.TotalPaid ?? 0m) : null,
            PaymentMethods = paid
                .GroupBy(o => GetPaymentMethod(o) ?? "Necunoscut")
                .ToDictionary(g => g.Key, g => g.Count())
        };

        var dto = new DailyCloseDto
        {
            Date = day,
            Lines = lines,
            Totals = totals
        };

        return Ok(dto);
    }

    [HttpGet("closeout-range")]
    public async Task<IActionResult> GetCloseoutRange([FromQuery] int days = 7)
    {
        var roTz = TimeZoneInfo.FindSystemTimeZoneById("E. Europe Standard Time");
        var nowRo = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, roTz).Date;
        var start = nowRo.AddDays(-Math.Max(0, days - 1));

        var all = await _orderService.GetAllAsync();
        var byDay = all.Where(o => {
            var d = ToRo(o.PlacedAt, roTz).Date;
            return d >= start && d <= nowRo;
        })
            .GroupBy(o => ToRo(o.PlacedAt, roTz).Date)
            .OrderByDescending(g => g.Key);

        var list = new List<DailyCloseDto>();

        foreach (var g in byDay)
        {
            var dayOrders = g.ToList();
            var paid = dayOrders.Where(o => IsCompleted(o.Status) && IsPaid(o)).ToList();

            var lines = paid
                .SelectMany(o => (o.Items ?? new List<OrderItemDto>())
                    .Select(i => new { o.Id, i.ProductName, i.Quantity, UnitPrice = GetUnitPrice(i) }))
                .GroupBy(x => x.ProductName)
                .Select(pg =>
                {
                    var qty = pg.Sum(x => x.Quantity);
                    decimal? unit = pg.Select(x => x.UnitPrice).Where(p => p.HasValue).Select(p => p!.Value).DefaultIfEmpty().FirstOrDefault();
                    bool hasAnyPrice = pg.Any(x => x.UnitPrice.HasValue);
                    decimal? total = hasAnyPrice ? pg.Sum(x => x.UnitPrice.GetValueOrDefault() * x.Quantity) : null;

                    return new DailyCloseLineDto
                    {
                        ProductName = pg.Key,
                        OrdersCount = pg.Select(x => x.Id).Distinct().Count(),
                        Quantity = qty,
                        UnitPrice = unit == 0 ? null : unit,
                        TotalPaid = total
                    };
                })
                .OrderByDescending(l => l.Quantity)
                .ThenBy(l => l.ProductName)
                .ToList();

            var totals = new DailyCloseTotalsDto
            {
                OrdersPaid = paid.Count,
                ItemsPaid = paid.SelectMany(o => o.Items ?? new()).Sum(i => i.Quantity),
                Revenue = lines.Any(l => l.TotalPaid.HasValue) ? lines.Sum(l => l.TotalPaid ?? 0m) : null,
                PaymentMethods = paid
                    .GroupBy(o => GetPaymentMethod(o) ?? "Necunoscut")
                    .ToDictionary(pg => pg.Key, pg => pg.Count())
            };

            list.Add(new DailyCloseDto { Date = g.Key, Lines = lines, Totals = totals });
        }

        return Ok(list);
    }

    private static DateTime ToRo(DateTime utc, TimeZoneInfo roTz)
        => utc.Kind == DateTimeKind.Utc ? TimeZoneInfo.ConvertTimeFromUtc(utc, roTz) : utc;

    private static bool IsCompleted(string status)
        => (status ?? "").Trim().Equals("Done", StringComparison.OrdinalIgnoreCase)
        || (status ?? "").Trim().Equals("Completed", StringComparison.OrdinalIgnoreCase);

    private static bool IsPaid(OrderDto o)
        => (o as dynamic).PaymentStatus == "Paid"
        || (o as dynamic).IsPaid == true;

    private static string? GetPaymentMethod(OrderDto o)
        => (o as dynamic).PaymentMethod;

    private static decimal? GetUnitPrice(OrderItemDto i)
        => (i as dynamic).UnitPrice;
}
