﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text.Json.Serialization;

namespace Shared
{
    public class OrderDto
    {
        public int Id { get; set; }
        public DateTime? Date { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))] 
        public OrderStatus Status { get; set; }
        public List<OrderLine>? OrderLines { get; set; }
        public int CustomerId { get; set; }
        
        public enum OrderStatus
        {
            Tentative,
            Completed,
            Cancelled,
            Shipped,
            Paid
        }
    }
    
    

    public class OrderLine
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int OrderId { get; set; }
        public int Quantity { get; set; }
    }
}