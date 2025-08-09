using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP25.Domain.Models
{
    public class PushSubscriptionEntity : BaseEntity
    {
        public string UserId { get; set; } = default!;
        public string Role { get; set; } = default!;
        public string Endpoint { get; set; } = default!;
        public string P256dh { get; set; } = default!;
        public string Auth { get; set; } = default!;
    }
}
