using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP25.Business.ModelDTOs
{
    public class GroupedOrdersDto
    {
        public List<OrderDto> Active { get; set; } = new();
        public List<OrderDto> Completed { get; set; } = new();
        public List<OrderDto> Cancelled { get; set; } = new();
        public Dictionary<string, List<OrderDto>> ArchivedGrouped { get; set; } = new();
    }

}
