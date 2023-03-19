namespace Shared
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public Category Category { get; set; }
        public int ItemsInStock { get; set; }
        public int ItemsReserved { get; set; }
    }

    public enum Category
    {
        Women,
        Men,
        Home,
        Beauty
    }
}
