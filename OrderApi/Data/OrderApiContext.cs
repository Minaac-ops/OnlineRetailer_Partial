using Microsoft.EntityFrameworkCore;
using OrderApi.Models;
using Shared;

namespace OrderApi.Data
{
    public class OrderApiContext : DbContext
    {
        public OrderApiContext(DbContextOptions<OrderApiContext> options)
            : base(options)
        {
        }

        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderLine> OrderLines { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>()
                .HasMany(order => order.OrderLines)
                .WithOne()
                .HasForeignKey(ol => ol.OrderId);
        }
    }
}
