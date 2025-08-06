using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP25.Domain.Models
{
    public enum WorkZone
    {
        None = 0,
        Pool = 1,
        Kitchen = 2
    }

    public enum OrderStatus
    {
        Pending = 0,
        InProgress = 1,
        Done = 2,
        Cancelled = 3
    }
}
