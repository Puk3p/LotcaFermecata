using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using SP25.Domain.Models;

namespace SP25.Domain.Context
{
    public class MyDbContext : IdentityDbContext<ApplicationUser>
    {
        private readonly string _windowsConnectionString = @"Server=localhost\SQLEXPRESS;Database=LotcaFermecata;Trusted_Connection=True;Encrypt=False";

        public DbSet<WorkZoneAuditLog> WorkZoneAuditLogs { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(_windowsConnectionString);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}
