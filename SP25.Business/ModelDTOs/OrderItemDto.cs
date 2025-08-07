using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SP25.Business.ModelDTOs
{
    public class OrderItemDto
    {
        [JsonPropertyName("productName")]
        public string ProductName { get; set; }

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }
    }
}
