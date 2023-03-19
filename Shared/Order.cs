using System;
using System.Collections.Generic;

namespace Shared
{
    public class Order
    {
        public int Id { get; set; }
        public DateTime? Date { get; set; }
        public Status Status { get; set; }
        public List<OrderLine> OrderLines { get; set; }
        public int CustomerId { get; set; }

        //orders should be able to contain multiple products

        //public OrderStatus Status {get;set;}
        //public IList<OrderLine> OrderLines {get;set;}

    //Order {Date} 1..*
    //Orderline {Quantity} *..1
    //Producy {}
    }

    public class OrderLine
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public enum Status
    {
        Completed,
        Cancelled,
        Shipped,
        Paid
    }
}