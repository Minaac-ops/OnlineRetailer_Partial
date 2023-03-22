using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ProductApi.Data;
using ProductApi.Models;
using Shared;

namespace ProductApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IRepository<Product> repository;
        private IConverter<Product, ProductDto> _converter;

        public ProductsController(IRepository<Product> repos, IConverter<Product, ProductDto> converter)
        {
            _converter = converter;
            repository = repos;
        }

        // GET products
        [HttpGet]
        public async Task<IEnumerable<ProductDto>> Get()
        {
            try
            {
                var products = await repository.GetAll();
                return products.Select(p => _converter.Convert(p));
            }
            catch (Exception e)
            {
                throw new Exception("Products couldn't be displayed due to error " + e.Message);
            }
        }

        // GET products/5
        [HttpGet("{id}", Name = "GetProduct")]
        public async Task<ProductDto> Get(int id)
        {
            try
            {
                var item = await repository.Get(id);
                return _converter.Convert(item);
            }
            catch (Exception e)
            {
                throw new Exception("Product with id " + id + " couldn't be displayed due to error " + e.Message);
            }
        }

        // POST products
        [HttpPost]
        public async Task<ProductDto> Post([FromBody] ProductDto productDto)
        {
            try
            {
                if (productDto == null) throw new Exception("Fill out product details.");

                var newProduct = await repository.Add(_converter.Convert(productDto));

                return _converter.Convert(newProduct);
            }
            catch (Exception e)
            {
                throw new Exception("Couldn't add product due to error " + e.Message);
            }
        }

        // PUT products/5
        [HttpPut("{id}")]
        public async Task Put(int id, [FromBody] ProductDto productDto)
        {
            try
            {
                if (productDto == null || productDto.Id != id)
                {
                    throw new Exception("Product or product id has to be filled out.");
                }

                await repository.Edit(id, _converter.Convert(productDto));
            }
            catch (Exception e)
            {
                throw new Exception("Couldn't edit product due to error " + e.Message);
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