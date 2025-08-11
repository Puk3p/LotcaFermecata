using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP25.Business.Services.Contracts
{
    public interface IPriceCatalog
    {
        decimal? GetPriceOrNull(string? productName);
    }
}
