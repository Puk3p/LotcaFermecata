using Microsoft.AspNetCore.Identity;
using SP25.Business.Services.Contracts;
using SP25.Domain.Context;
using SP25.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP25.Business.Services.Implementations
{
    public class WorkZoneService : IWorkZoneService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly MyDbContext _dbContext;

        public WorkZoneService(UserManager<ApplicationUser> userManager, MyDbContext dbContext)
        {
            _userManager = userManager;
            _dbContext = dbContext;
        }

        public async Task<bool> SetZoneAsync(string userId, string zone)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return false;

            if (!Enum.TryParse<WorkZone>(zone, true, out var chosenZone) || chosenZone == WorkZone.None)
                return false;

            user.CurrentZone = chosenZone;
            await _userManager.UpdateAsync(user);

            _dbContext.WorkZoneAuditLogs.Add(new WorkZoneAuditLog
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Zone = chosenZone,
                ChangedAt = DateTime.UtcNow
            });
            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<WorkZone?> GetCurrentZoneAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            return user?.CurrentZone;
        }
    }
}