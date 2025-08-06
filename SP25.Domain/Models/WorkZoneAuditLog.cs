using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP25.Domain.Models
{
    public class WorkZoneAuditLog : BaseEntity
    {
        public string UserId { get; set; }
        public WorkZone Zone { get; set; }
        public DateTime ChangedAt { get; set; }
    }
}
