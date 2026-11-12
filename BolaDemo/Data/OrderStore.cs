using BolaDemo.Models;

namespace BolaDemo.Data
{
    public class OrderStore
    {
        public static readonly List<Order> _orders = new List<Order>
        {
            new Order { Id = 1, UserId = 1, OrderDate = DateTime.Now.AddDays(-2), Address = "123 Main St", TotalAmount = 499, Name = "Hans"},
            new Order { Id = 2, UserId = 2, OrderDate = DateTime.Now.AddDays(-1), Address = "456 Elm St", TotalAmount = 1299,  Name = "Per"},
            new Order { Id = 3, UserId = 1, OrderDate = DateTime.Now, Address = "123 Main St", TotalAmount = 89,  Name = "Anna"}

        };
        public Order? GetOrderById(int id)
        {
            return _orders.FirstOrDefault(o => o.Id == id);
        }
        public Order? GetByIdForUser(int id, int userId)
        {
            return _orders.FirstOrDefault(o => o.Id == id && o.UserId == userId);
        }
    }
}