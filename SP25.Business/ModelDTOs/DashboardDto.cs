using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP25.Business.ModelDTOs
{
    public class DashboardDto
    {
        public List<KpiDto> Kpis { get; set; }
        public ProductTrendDto ProductOfDay { get; set; }
        public List<ActivityDto> Recent { get; set; }
        public List<ZoneStatDto> PeakZones { get; set; }

        public List<HourStatDto> PeakHours { get; set; } = new();
    }

    public class KpiDto
    {
        public string Label { get; set; }
        public int Value { get; set; }
        public double Trend { get; set; } // % față de media zilelor trecute
        public string Icon { get; set; }
    }

    public class ProductTrendDto
    {
        public string Name { get; set; }
        public int Count { get; set; }
        public double TrendPercent { get; set; }
    }

    public class ActivityDto
    {
        public string Title { get; set; }
        public string Badge { get; set; }
        public string Type { get; set; }
        public string When { get; set; }
        public string Zone { get; set; }
    }

    public class ZoneStatDto
    {
        public string Zone { get; set; } = "";
        public int Orders { get; set; }
        public double PercentOfTotal { get; set; }

        public int? PeakHour { get; set; }
        public int? PeakHourOrders { get; set; }
        public string? PeakHourLabel =>
            PeakHour.HasValue ? $"{PeakHour:D2}:00–{(PeakHour.Value + 1) % 24:D2}:00" : null;
    }

    public class HourStatDto
    {
        public int Hour { get; set; }
        public int Orders { get; set; }
        public double PercentOfTotal { get; set; }
        public string Label => $"{Hour:D2}:00–{(Hour + 1) % 24:D2}:00";
    }
}
