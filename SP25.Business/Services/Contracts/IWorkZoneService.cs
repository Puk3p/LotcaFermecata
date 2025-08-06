using SP25.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP25.Business.Services.Contracts
{
    public interface IWorkZoneService
    {
        Task<bool> SetZoneAsync(string userId, string zone);
        Task<WorkZone?> GetCurrentZoneAsync(string userId);
    }
}
