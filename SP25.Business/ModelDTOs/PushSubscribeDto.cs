using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP25.Business.ModelDTOs
{
    public class PushSubscribeDto
    {
        public string UserId { get; set; } = default!;
        public string Role { get; set; } = default!;
        public WebPushSubscriptionDto Subscription { get; set; } = default!;
    }

    public class WebPushSubscriptionDto
    {
        public string Endpoint { get; set; } = default!;
        public WebPushKeysDto Keys { get; set; } = default!;
    }

    public class WebPushKeysDto
    {
        public string P256dh { get; set; } = default!;
        public string Auth { get; set; } = default!;
    }
}

