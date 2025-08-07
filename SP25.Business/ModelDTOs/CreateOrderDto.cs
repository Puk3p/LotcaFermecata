using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SP25.Business.ModelDTOs
{
    public class CreateOrderDto
    {
        [JsonPropertyName("clientName")]
        public string? ClientName { get; set; }

        [JsonPropertyName("clientPhone")]
        public string? ClientPhone { get; set; }

        [JsonPropertyName("targetZone")]
        public string TargetZone { get; set; }

        [JsonPropertyName("items")]
        public List<OrderItemDto> Items { get; set; }

        [JsonPropertyName("placedByUserId")]
        public string PlacedByUserId { get; set; }
    }
}
