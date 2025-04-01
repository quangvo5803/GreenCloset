using DataAccess.Models;

namespace Repository.Interface
{
    public interface IItemImageRepository : IRepository<ItemImage>
    {
        void Update(ItemImage itemImage);
    }
}
