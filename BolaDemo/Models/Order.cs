namespace BolaDemo.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime OrderDate { get; set; }
        public string Address { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }

    }
}
