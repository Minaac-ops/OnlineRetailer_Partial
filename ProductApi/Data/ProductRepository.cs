using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using ProductApi.Models;
using Shared;

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
            return await db.Products.FirstOrDefaultAsync(p => p.Id == id);
        }

        async Task<IEnumerable<Product>> IRepository<Product>.GetAll()
        {
            return await db.Products.ToListAsync();
        }

        async Task IRepository<Product>.Remove(int id)
        {
            var product = await db.Products.FirstOrDefaultAsync(p => p.Id == id);
            db.Products.Remove(product);
            await db.SaveChangesAsync();
        }
    }
}
