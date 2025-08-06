using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP25.Domain.Models
{
    public class Order : BaseEntity
    {
        public string PlacedByUserId { get; set; }
        public string? ClientName { get; set; }
        public string? ClientPhone { get; set; }
        public WorkZone TargetZone { get; set; }
        public DateTime PlacedAt { get; set; }
        public OrderStatus Status { get; set; }


        public ICollection<OrderItem> Items { get; set; }
    }
}
