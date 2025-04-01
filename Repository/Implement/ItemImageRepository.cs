using DataAccess.Data;
using DataAccess.Models;
using Repository.Interface;

namespace Repository.Implement
{
    public class ItemImageRepository : Repository<ItemImage>, IItemImageRepository
    {
        private readonly ApplicationDbContext _db;

        public ItemImageRepository(ApplicationDbContext db)
            : base(db)
        {
            _db = db;
        }

        public void Update(ItemImage itemImage)
        {
            _db.ItemImages.Update(itemImage);
        }
    }
}
