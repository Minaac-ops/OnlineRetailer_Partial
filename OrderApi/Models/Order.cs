using System;
using System.Collections.Generic;
using Shared;

namespace OrderApi.Models
{
    public class Order
    {
        public int Id { get; set; }
        public DateTime? Date { get; set; }
        
        public OrderDto.OrderStatus Status { get; set; }
        public List<OrderLine> OrderLines { get; set; }
        public int CustomerId { get; set; }
    }
}