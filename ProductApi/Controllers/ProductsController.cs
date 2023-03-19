using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using ProductApi.Data;
using Shared;

namespace ProductApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IRepository<Product> repository;

        public ProductsController(IRepository<Product> repos)
        {
            repository = repos;
        }

        // GET products
        [HttpGet]
        public IEnumerable<Product> Get()
        {
            return repository.GetAll();
        }

        // GET products/5
        [HttpGet("{id}", Name="GetProduct")]
        public IActionResult Get(int id)
        {
            var item = repository.Get(id);
            if (item == null)
            {
                return NotFound();
            }
            return new ObjectResult(item);
        }

        // POST products
        [HttpPost]
        public IActionResult Post([FromBody]Product product)
        {
            if (product == null)
            {
                return BadRequest();
            }

            var newProduct = repository.Add(product);

            return CreatedAtRoute("GetProduct", new { id = newProduct.Id }, newProduct);
        }

        // PUT products/5
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody]Product product)
        {
            if (product == null || product.Id != id)
            {
                return BadRequest();
            }

            repository.Edit(id, product);
            return new NoContentResult();
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
