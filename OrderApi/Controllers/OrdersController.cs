using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OrderApi.Data;
using RestSharp;
using Shared;

namespace OrderApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IRepository<Order> repository;

        public OrdersController(IRepository<Order> repos)
        {
            repository = repos;
        }

        // GET: orders
        [HttpGet]
        public async Task<IEnumerable<Order>> Get()
        {
            try
            {
                return await repository.GetAll();
            }
            catch (Exception e)
            {
                throw new Exception("Orders couldn't be displayed due to error " + e.Message);
            }
            
        }

        // GET orders/5
        [HttpGet("{id}", Name = "GetOrder")]
        public async Task<Order> Get(int id)
        {
            try
            {
                var item = await repository.Get(id);
                return item;
            }
            catch (Exception e)
            {
                throw new Exception("Order with id "+id+" couldn't be displayed due to error "+e.Message);
            }
            
        }

        // POST orders
        [HttpPost]
        public async Task<Order> Post([FromBody]Order order)
        {
            try
            {
                //Checking if order is null
                if (order == null) throw new Exception("Fill out order details.");
                
                //Calling customer service to find the customer to check if customer is created
                var customerService = new RestClient("http://host.docker.internal:8000");
                var requestCust = new RestRequest("Customer/"+order.CustomerId);
                var responseCust = await customerService.GetAsync<Customer>(requestCust);
                if (responseCust == null) throw new Exception("Not able to get a response from ProductService");
                
                // Call ProductApi to get the products ordered.
                var productService = new RestClient("http://host.docker.internal:8002");
                
                foreach (var item in order.OrderLines)
                {
                    var request = new RestRequest("Products/"+item.ProductId);
                    var response = await productService.GetAsync<Product>(request);
                    if (response == null) throw new Exception("Not able to get a response from ProductService");

                    if (item.Quantity > response.ItemsInStock - response.ItemsReserved) continue;
                    // reduce the number of items in stock for the ordered product,
                    // and create a new order.
                    response.ItemsReserved += item.Quantity;
                    var updateRequest = new RestRequest(response.Id.ToString());
                    updateRequest.AddJsonBody(response);
                    var updateResponse = productService.PutAsync(updateRequest);
                    updateResponse.Wait();

                    if (!updateResponse.IsCompletedSuccessfully) continue;
                    var newOrder = await repository.Add(order);
                    return newOrder;
                }
            }
            catch (Exception e)
            {
                throw new Exception("Order couldn't be created due to error "+ e.Message);
            }
            return null;
        }

    }
}
