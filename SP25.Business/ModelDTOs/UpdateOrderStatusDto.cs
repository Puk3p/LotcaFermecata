using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP25.Business.ModelDTOs
{
    public class UpdateOrderStatusDto
    {
        public Guid OrderId { get; set; }
        public string NewStatus { get; set; }
    }
}
