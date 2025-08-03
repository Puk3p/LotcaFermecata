using Microsoft.EntityFrameworkCore;
using SP25.Domain.Models;

namespace SP25.Domain.Context
{
    public class MyDbContext : DbContext
    {
        private readonly string _windowsConnectionString = @"Server=.\SQLExpress;Database=SP25_Database;Trusted_Connection=True;TrustServerCertificate=true";

        public DbSet<TestModel> TestModels { get; set; }

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
