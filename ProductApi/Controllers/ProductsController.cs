using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ProductApi.Data;
using Shared;

namespace ProductApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IRepository<ProductDto> repository;

        public ProductsController(IRepository<ProductDto> repos)
        {
            repository = repos;
        }

        // GET products
        [HttpGet]
        public async Task<IEnumerable<ProductDto>> Get()
        {
            try
            {
                return await repository.GetAll();
            }
            catch (Exception e)
            {
                throw new Exception("Products couldn't be displayed due to error " + e.Message);
            }
        }

        // GET products/5
        [HttpGet("{id}", Name="GetProduct")]
        public async Task<ProductDto> Get(int id)
        {
            try
            {
                var item = await repository.Get(id);
                return item;
            }
            catch (Exception e)
            {
                throw new Exception("Product with id "+id+" couldn't be displayed due to error "+e.Message);
            }
            
        }

        // POST products
        [HttpPost]
        public async Task<ProductDto> Post([FromBody]ProductDto productDto)
        {
            try
            {
                if (productDto == null) throw new Exception("Fill out product details.");
                
                var newProduct = await repository.Add(productDto);

                return newProduct;
            }
            catch (Exception e)
            {
                throw new Exception("Couldn't add product due to error " + e.Message);
            }
            }

        // PUT products/5
        [HttpPut("{id}")]
        public async Task Put(int id, [FromBody]ProductDto productDto)
        {
            try
            {
                if (productDto == null || productDto.Id != id)
                {
                    throw new Exception("Product or product id has to be filled out.");
                }
                await repository.Edit(id, productDto);
            }
            catch (Exception e)
            {
                throw new Exception("Couldn't edit product due to error " +e.Message);
            }
        }

        // DELETE products/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            if (repository.Get(id) == null)
            {
                return NotFound();
            }

            repository.Remove(id);
            return new NoContentResult();
        }
    }
}
