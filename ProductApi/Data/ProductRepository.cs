using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Monitoring;
using ProductApi.Models;

namespace ProductApi.Data
{
    public class ProductRepository : IRepository<Product>
    {
        private readonly ProductApiContext db;

        public ProductRepository(ProductApiContext context)
        {
            db = context;
        }

        async Task<Product> IRepository<Product>.Add(Product entity)
        {
            MonitorService.Log.Here().Debug("ProductRepository: Add");
            var newProduct =await db.Products.AddAsync(entity);
            await db.SaveChangesAsync();
            return newProduct.Entity;
        }

        async Task IRepository<Product>.Edit(Product entity)
        {
            if (entity == null) return;
            
            db.Entry(entity).State = EntityState.Modified;
            await db.SaveChangesAsync();
        }

        async Task<Product> IRepository<Product>.Get(int id)
        {
            MonitorService.Log.Here().Debug("ProductRepository: Get");
            return await db.Products.FirstOrDefaultAsync(p => p.Id == id);
        }

        async Task<IEnumerable<Product>> IRepository<Product>.GetAll()
        {
            MonitorService.Log.Here().Debug("ProductRepository: GetAll");
            return await db.Products.ToListAsync();
        }

        async Task IRepository<Product>.Remove(int id)
        {
            MonitorService.Log.Here().Debug("ProductRepository: Remove");
            var product = await db.Products.FirstOrDefaultAsync(p => p.Id == id);
            db.Products.Remove(product);
            await db.SaveChangesAsync();
        }
    }
}
