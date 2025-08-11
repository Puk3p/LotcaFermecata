using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP25.Business.ModelDTOs
{
    public class DailyCloseDto
    {
        public DateTime Date { get; set; }
        public List<DailyCloseLineDto> Lines { get; set; } = new();
        public DailyCloseTotalsDto Totals { get; set; } = new();
    }

    public class DailyCloseLineDto
    {
        public string ProductName { get; set; } = "";
        public int OrdersCount { get; set; }
        public int Quantity { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? TotalPaid { get; set; }
    }

    public class DailyCloseTotalsDto
    {
        public int OrdersPaid { get; set; }
        public int ItemsPaid { get; set; }
        public decimal? Revenue { get; set; }
        public Dictionary<string, int> PaymentMethods { get; set; } = new();
    }
}