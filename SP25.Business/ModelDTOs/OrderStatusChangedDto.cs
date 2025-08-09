using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP25.Business.ModelDTOs
{
    public class OrderStatusChangedDto
    {
        public string OrderId { get; set; } = default!;
        public string NewStatus { get; set; } = default!;
    }
}