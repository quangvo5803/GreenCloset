using DataAccess.Data;
using DataAccess.Models;
using Repository.Interface;

namespace Repository.Implement
{
    public class CartRepository : Repository<Cart>, ICartRepository
    {
        private readonly ApplicationDbContext _db;

        public CartRepository(ApplicationDbContext db)
            : base(db)
        {
            _db = db;
        }

        public void Update(Cart cart)
        {
            _db.Carts.Update(cart);
        }
    }
}
