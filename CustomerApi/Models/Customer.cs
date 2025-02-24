namespace CustomerApi.Models
{
    public class Customer
    {
        public int Id { get; set; }
        public string? CompanyName { get; set; }
        public string? Email { get; set; }
        public int PhoneNo { get; set; }
        public string? BillingAddress { get; set; }
        public string? ShippingAddress { get; set; }
        public bool CreditStanding { get; set; }
    }
}