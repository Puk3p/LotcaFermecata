using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SP25.Business.ModelDTOs;
using SP25.Business.Services.Contracts;
using SP25.Domain.Context;
using SP25.Domain.Models;
using SP25.Domain.Repository;
using System;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using WebPush;

namespace SP25.Business.Services.Implementations
{
    public class NotificationService : INotificationService
    {
        private readonly IRepository<PushSubscriptionEntity> _repo;
        private readonly IConfiguration _cfg;
        private readonly ILogger<NotificationService> _logger;
        private readonly MyDbContext _dbContext;

        public NotificationService(
            IRepository<PushSubscriptionEntity> repo,
            IConfiguration cfg,
            ILogger<NotificationService> logger,
            MyDbContext dbContext)
        {
            _repo = repo;
            _cfg = cfg;
            _logger = logger;
            _dbContext = dbContext;
        }

        public async Task SubscribeAsync(PushSubscribeDto dto)
        {
            _logger.LogInformation("SUBSCRIBE: start. user={user} role={role} endpoint={ep}",
                dto?.UserId, dto?.Role, dto?.Subscription?.Endpoint);

            if (dto?.Subscription == null)
            {
                _logger.LogWarning("SUBSCRIBE: payload invalid (subscription null)");
                return;
            }

            var all = await _repo.GetAllAsync();
            _logger.LogInformation("SUBSCRIBE: current subs count before upsert = {all.Count}");

            var existing = all.FirstOrDefault(s => s.Endpoint == dto.Subscription.Endpoint);
            if (existing != null)
            {
                existing.UserId = dto.UserId;
                existing.Role = dto.Role;
                existing.P256dh = dto.Subscription.Keys.P256dh;
                existing.Auth = dto.Subscription.Keys.Auth;

                await _repo.UpdateAsync(existing);
                await _repo.SaveChangesAsync();
                _logger.LogInformation("SUBSCRIBE: updated existing subscription for endpoint={ep}", existing.Endpoint);
                return;
            }

            var entity = new PushSubscriptionEntity
            {
                UserId = dto.UserId,
                Role = dto.Role,
                Endpoint = dto.Subscription.Endpoint,
                P256dh = dto.Subscription.Keys.P256dh,
                Auth = dto.Subscription.Keys.Auth
            };

            await _repo.AddAsync(entity);
            await _repo.SaveChangesAsync();
            _logger.LogInformation("SUBSCRIBE: inserted new subscription. endpoint={ep}", entity.Endpoint);

            all = await _repo.GetAllAsync();
            _logger.LogInformation("SUBSCRIBE: subs count AFTER upsert = {all.Count}");
        }

        // NotificationService.cs
        public async Task NotifyOrderStatusChangedAsync(string orderId, string newStatus, string? notifyRole)
        {
            _logger.LogInformation($"PUSH: preparing send (order={orderId}, status={newStatus}, role={notifyRole})");

            var vapidPublic = _cfg["Vapid:PublicKey"];
            var vapidPrivate = _cfg["Vapid:PrivateKey"];
            var vapidSubject = _cfg["Vapid:Subject"] ?? "mailto:admin@exemplu.ro";

            if (string.IsNullOrWhiteSpace(vapidPublic) || string.IsNullOrWhiteSpace(vapidPrivate))
            {
                _logger.LogWarning("PUSH: VAPID keys missing (Vapid:PublicKey / Vapid:PrivateKey).");
                return;
            }

            var all = await _repo.GetAllAsync();
            _logger.LogInformation("PUSH: total subscriptions in DB = {count}", all.Count());

            var targets = string.IsNullOrWhiteSpace(notifyRole)
                ? all.ToList()
                : all.Where(s => string.Equals(s.Role, notifyRole, StringComparison.OrdinalIgnoreCase)).ToList();

            _logger.LogInformation($"PUSH: targets for role {(notifyRole ?? "(all)")} = {targets.Count}");
            if (targets.Count == 0) return;

            var client = new WebPushClient();
            var vapid = new VapidDetails(vapidSubject, vapidPublic, vapidPrivate);


            string ? clientName = null;
            if (Guid.TryParse(orderId, out var oid))
            {
                var order = await _dbContext.Orders
                    .AsNoTracking()
                    .FirstOrDefaultAsync(o => o.Id == oid);

                clientName = order?.ClientName;
            }

            var title = !string.IsNullOrWhiteSpace(clientName)
                ? $"Comanda lui {clientName}"
                : $"Comanda #{orderId}";

            var payload = System.Text.Json.JsonSerializer.Serialize(new
            {
                title,
                body = $"Vino la bucătărie. Te rugăm să o preiei 📦",
                icon = "/assets/icons/icon-192x192.png",
                badge = "/assets/icons/badge-72x72.png",
                image = "/assets/hero/order-done.png",
                tag = $"order-{orderId}",
                renotify = true,
                requireInteraction = true,
                actions = new[]
                {
                    new { action = "open", title = "Deschide" },
                    new { action = "ack", title = "Am preluat" }
                },
                data = new
                {
                    url = $"/orders/view/{orderId}?tab=active",
                    orderId
                }
            });

            foreach (var t in targets.ToList())
            {
                var sub = new PushSubscription(t.Endpoint, t.P256dh, t.Auth);

                try
                {
                    await client.SendNotificationAsync(sub, payload, vapid);
                    _logger.LogInformation($"PUSH: sent OK to endpoint={t.Endpoint}");
                }
                catch (WebPushException ex)
                {
                    // Unele versiuni nu au ResponseBody; logăm ce avem sigur: StatusCode + Message
                    _logger.LogError(ex, $"PUSH: WebPushException endpoint={t.Endpoint}, status={(int)ex.StatusCode} {ex.StatusCode}, message={ex.Message}");

                    if (ex.StatusCode == HttpStatusCode.Gone || ex.StatusCode == HttpStatusCode.NotFound)
                    {
                        _logger.LogWarning($"PUSH: endpoint expired/404 -> removing endpoint={t.Endpoint}");
                        await _repo.DeleteAsync(t);
                        await _repo.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"PUSH: unexpected error for endpoint={t.Endpoint}");
                }
            }
        }
        public async Task NotifyOrderStatusChangedAsync(string orderId, string newStatus)
        {
            await NotifyOrderStatusChangedAsync(orderId, newStatus, notifyRole: "BAR");
        }
    }
}
