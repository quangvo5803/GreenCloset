using DataAccess.Models;

namespace BussinessLayer.Interface
{
    public interface IItemImageService
    {
        ItemImage? GetItemImageById(int id);
        void RemoveItemImage(ItemImage itemImage);
    }
}
