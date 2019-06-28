namespace ClassLibrary.Classes
{
    public class ProductForManager
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public int Count { get; set; }
        public int CountInOrders { get; set; }
        public int CountInCarts { get; set; }
    }
}