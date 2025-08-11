using System.Text.Json.Serialization;

namespace SP25.Business.ModelDTOs
{
    public class ProductSummaryDto
    {
        public string ProductName { get; set; } = "";
        public int OrdersCount { get; set; }
        public int Quantity { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? TotalPaid { get; set; }
    }

    public class DayTotalsDto
    {
        public int OrdersPaid { get; set; }
        public int ItemsPaid { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public decimal? Revenue { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, int> PaymentMethods { get; set; } = new();
    }

    public class DayArchiveDto
    {
        public List<OrderDto> Orders { get; set; } = new();
        public List<ProductSummaryDto> Summary { get; set; } = new();
        public DayTotalsDto Totals { get; set; } = new();
    }
}
