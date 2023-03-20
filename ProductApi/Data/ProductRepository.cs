using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Shared;

namespace ProductApi.Data
{
    public class ProductRepository : IRepository<ProductDto>
    {
        private readonly ProductApiContext db;

        public ProductRepository(ProductApiContext context)
        {
            db = context;
        }

        async Task<ProductDto> IRepository<ProductDto>.Add(ProductDto entity)
        {
            var newProduct =await db.Products.AddAsync(entity);
            await db.SaveChangesAsync();
            return newProduct.Entity;
        }

        async Task IRepository<ProductDto>.Edit(int id,ProductDto entity)
        {
            var productToModify = await db.Products.FindAsync(id);

            if (productToModify == null) return;
            
            productToModify.Name = entity.Name;
            productToModify.Price = entity.Price;
            productToModify.ItemsInStock = entity.ItemsInStock;
            productToModify.ItemsReserved = entity.ItemsReserved;
            
            db.Entry(productToModify).State = EntityState.Modified;
            await db.SaveChangesAsync();
        }

        async Task<ProductDto> IRepository<ProductDto>.Get(int id)
        {
            return await db.Products.FirstOrDefaultAsync(p => p.Id == id);
        }

        async Task<IEnumerable<ProductDto>> IRepository<ProductDto>.GetAll()
        {
            return await db.Products.ToListAsync();
        }

        void IRepository<ProductDto>.Remove(int id)
        {
            var product = db.Products.FirstOrDefault(p => p.Id == id);
            db.Products.Remove(product);
            db.SaveChanges();
        }
    }
}
