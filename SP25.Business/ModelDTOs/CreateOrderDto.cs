using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP25.Business.ModelDTOs
{
    public class CreateOrderDto
    {
        public string? ClientName { get; set; }
        public string? ClientPhone { get; set; }
        public string TargetZone { get; set; }
        public List<OrderItemDto> Items { get; set; }
    }
}
