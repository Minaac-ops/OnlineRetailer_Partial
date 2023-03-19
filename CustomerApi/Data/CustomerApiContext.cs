using Microsoft.EntityFrameworkCore;
using Shared;

namespace CustomerApi.Data
{
    public class CustomerApiContext : DbContext
    {
        public CustomerApiContext(DbContextOptions<CustomerApiContext> options)
            : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<OrderLine> OrderLines { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>()
                .HasMany(order => order.OrderLines).WithOne();

            modelBuilder.Entity<Order>()
                .Navigation(o => o.OrderLines)
                .UsePropertyAccessMode(PropertyAccessMode.Property);
        }
    }
}