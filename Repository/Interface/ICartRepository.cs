using DataAccess.Models;

namespace Repository.Interface
{
    public interface ICartRepository : IRepository<Cart>
    {
        void Update(Cart cart);
    }
}
