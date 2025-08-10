using Microsoft.AspNetCore.Mvc;
using SP25.Business.ModelDTOs;
using SP25.Business.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SP25.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public DashboardController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        /// <summary>
        /// role este opțional (ex: "BAR" sau "BUCATARIE"). Dacă îl trimiți, filtrăm doar comenzile relevante rolului (vezi funcția IsRelevantForRole).
        /// </summary>
        [HttpGet("today")]
        public async Task<IActionResult> GetTodayDashboard()
        {
            // Ora României
            var roTz = TimeZoneInfo.FindSystemTimeZoneById("E. Europe Standard Time");
            var nowRo = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, roTz);
            var today = nowRo.Date;
            var last7Days = today.AddDays(-7);

            var orders = (await _orderService.GetAllAsync()).ToList();

            // Împărțire: azi vs. ultimele 7 zile (exclus azi)
            var todayOrders = orders.Where(o => ToRoDate(o.PlacedAt, roTz).Date == today).ToList();
            var pastDaysOrders = orders.Where(o =>
            {
                var d = ToRoDate(o.PlacedAt, roTz).Date;
                return d >= last7Days && d < today;
            }).ToList();

            // --- KPI-uri ---
            int totalToday = todayOrders.Count;
            double totalPastAvg = GroupDailyAvg(pastDaysOrders, roTz, g => g.Count());

            int activeToday = todayOrders.Count(o => IsActive(o.Status));
            double activePastAvg = GroupDailyAvg(pastDaysOrders, roTz, g => g.Count(o => IsActive(o.Status)));

            int completedToday = todayOrders.Count(o => IsCompleted(o.Status));
            double completedPastAvg = GroupDailyAvg(pastDaysOrders, roTz, g => g.Count(o => IsCompleted(o.Status)));

            int cancelledToday = todayOrders.Count(o => IsCancelled(o.Status));
            double cancelledPastAvg = GroupDailyAvg(pastDaysOrders, roTz, g => g.Count(o => IsCancelled(o.Status)));

            // --- Produsul zilei (după cantitate) + trend față de media zilnică din ultimele 7 zile ---
            var productOfDay = todayOrders
                .SelectMany(o => (o.Items ?? new List<OrderItemDto>())
                    .Select(i => new { i.ProductName, i.Quantity }))
                .GroupBy(x => x.ProductName)
                .Select(g => new { Name = g.Key, Qty = g.Sum(x => x.Quantity) })
                .OrderByDescending(x => x.Qty)
                .FirstOrDefault();

            double productTrend = 0;
            if (productOfDay != null)
            {
                var pastPerDayForProduct = pastDaysOrders
                    .Select(o => new
                    {
                        Day = ToRoDate(o.PlacedAt, roTz).Date,
                        Items = o.Items ?? new List<OrderItemDto>()
                    })
                    .SelectMany(x => x.Items.Select(i => new { x.Day, i.ProductName, i.Quantity }))
                    .Where(x => x.ProductName == productOfDay.Name)
                    .GroupBy(x => x.Day)
                    .Select(g => g.Sum(x => x.Quantity))
                    .DefaultIfEmpty(0)
                    .Average();

                if (pastPerDayForProduct > 0)
                    productTrend = ((productOfDay.Qty - pastPerDayForProduct) / pastPerDayForProduct) * 100.0;
            }

            // --- Ore de vârf ale zilei (top 3 ore cu cele mai multe comenzi) ---
            var peakHours = todayOrders
                .GroupBy(o => ToRoDate(o.PlacedAt, roTz).Hour)
                .Select(g => new HourStatDto
                {
                    Hour = g.Key,
                    Orders = g.Count(),
                    PercentOfTotal = totalToday > 0 ? g.Count() * 100.0 / totalToday : 0
                })
                .OrderByDescending(h => h.Orders).ThenBy(h => h.Hour)
                .Take(3)
                .ToList();

            // --- Zone de vârf (cu ora de vârf pe fiecare zonă) ---
            var peakZones = todayOrders
                .GroupBy(o => o.TargetZone)
                .Select(g =>
                {
                    var zoneOrders = g.ToList();
                    var zoneTotal = zoneOrders.Count;

                    var zPeak = zoneOrders
                        .GroupBy(o => ToRoDate(o.PlacedAt, roTz).Hour)
                        .Select(hg => new { Hour = hg.Key, Cnt = hg.Count() })
                        .OrderByDescending(x => x.Cnt).ThenBy(x => x.Hour)
                        .FirstOrDefault();

                    return new ZoneStatDto
                    {
                        Zone = g.Key,
                        Orders = zoneTotal,
                        PercentOfTotal = totalToday > 0 ? zoneTotal * 100.0 / totalToday : 0,
                        PeakHour = zPeak?.Hour,
                        PeakHourOrders = zPeak?.Cnt
                    };
                })
                .OrderByDescending(z => z.Orders)
                .ToList();

            // --- Recent (ultimele 5) ---
            var recent = todayOrders
                .OrderByDescending(o => ToRoDate(o.PlacedAt, roTz))
                .Take(5)
                .Select(o => new ActivityDto
                {
                    Title = $"Comanda #{o.Id} - {o.Status}",
                    Badge = o.Status,
                    Type = MapStatusToType(o.Status),
                    When = ToRoDate(o.PlacedAt, roTz).ToString("HH:mm"),
                    Zone = o.TargetZone
                })
                .ToList();

            var dto = new DashboardDto
            {
                Kpis = new List<KpiDto>
        {
            new KpiDto { Label = "Comenzi azi",  Value = totalToday,     Trend = PercentDiff(totalToday, totalPastAvg),     Icon = "fa-box" },
            new KpiDto { Label = "Active acum",  Value = activeToday,    Trend = PercentDiff(activeToday, activePastAvg),   Icon = "fa-fire" },
            new KpiDto { Label = "Finalizate",   Value = completedToday, Trend = PercentDiff(completedToday, completedPastAvg), Icon = "fa-check-circle" },
            new KpiDto { Label = "Anulate",      Value = cancelledToday, Trend = PercentDiff(cancelledToday, cancelledPastAvg), Icon = "fa-ban" }
        },
                ProductOfDay = productOfDay == null ? null : new ProductTrendDto
                {
                    Name = productOfDay.Name,
                    Count = productOfDay.Qty,
                    TrendPercent = Math.Round(productTrend, 2)
                },
                Recent = recent,
                PeakZones = peakZones,
                PeakHours = peakHours
            };

            return Ok(dto);
        }

        private static double GroupDailyAvg(
            IEnumerable<OrderDto> src,
            TimeZoneInfo roTz,
            Func<IEnumerable<OrderDto>, int> selector)
                {
                    return src
                        .GroupBy(o => ToRoDate(o.PlacedAt, roTz).Date)
                        .Select(g => selector(g))
                        .DefaultIfEmpty(0)
                        .Average();
                }

        private static DateTime ToRoDate(DateTime utc, TimeZoneInfo roTz)
            => utc.Kind == DateTimeKind.Utc
                ? TimeZoneInfo.ConvertTimeFromUtc(utc, roTz)
                : utc;

        private static bool IsActive(string status)
        {
            // tratează robust: Pending/InProgress => active
            var s = status?.Trim() ?? "";
            return s.Equals("Pending", StringComparison.OrdinalIgnoreCase)
                || s.Equals("InProgress", StringComparison.OrdinalIgnoreCase)
                || s.Equals("Active", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsCompleted(string status)
        {
            var s = status?.Trim() ?? "";
            return s.Equals("Done", StringComparison.OrdinalIgnoreCase)
                || s.Equals("Completed", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsCancelled(string status)
        {
            var s = status?.Trim() ?? "";
            return s.Equals("Cancelled", StringComparison.OrdinalIgnoreCase)
                || s.Equals("Canceled", StringComparison.OrdinalIgnoreCase);
        }

        private static string MapStatusToType(string status)
        {
            if (IsCompleted(status)) return "done";
            if (IsCancelled(status)) return "cancelled";
            return "pending";
        }

        private static double PercentDiff(int today, double pastAvg)
        {
            if (pastAvg <= 0.0001) return today > 0 ? 100d : 0d;
            return Math.Round(((today - pastAvg) / pastAvg) * 100.0, 2);
        }


        /// <summary>
        /// Heuristică simplă: dacă rolul e BAR, arătăm comenzi fără mâncare (doar băuturi),
        /// dacă e BUCATARIE, arătăm comenzi care conțin mâncare.
        /// Ajustează după cum ai deja logică în FE.
        /// </summary>
        private static bool IsRelevantForRole(string role, OrderDto o)
        {
            var items = o.Items ?? new List<OrderItemDto>();
            bool hasFood = items.Any(i => ContainsFood(i.ProductName));

            if (role.Equals("BUCATARIE", StringComparison.OrdinalIgnoreCase))
                return hasFood;

            if (role.Equals("BAR", StringComparison.OrdinalIgnoreCase))
                return !hasFood;

            // role necunoscut => nu filtrăm
            return true;
        }

        private static bool ContainsFood(string? name)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;
            var n = name.ToLowerInvariant();
            string[] foodKeywords = {
                "șnițel","snitel","pui","porc","aripioare","pizza","mici","ceafă","ceafa",
                "cartofi","salată","salata","sos","chiflă","chifla","papanasi","gogoși","gogosi",
                "burger","paste","tocan","ciorb","fript","gratar"
            };
            return foodKeywords.Any(k => n.Contains(k));
        }
    }
}