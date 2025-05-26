using DataAccess.Models;

namespace BussinessLayer.Interface
{
    public interface IItemImageService
    {
        ItemImage? GetItemImageById(int id);
        Task RemoveItemImage(ItemImage itemImage);
    }
}
