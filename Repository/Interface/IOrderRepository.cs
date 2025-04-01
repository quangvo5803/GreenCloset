using DataAccess.Models;

namespace Repository.Interface
{
    public interface IOrderRepository : IRepository<Order>
    {
        void Update(Order order);
    }
}
