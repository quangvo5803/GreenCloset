using BussinessLayer.Interface;
using DataAccess.Models;
using Repository.Implement;
using Utility.Media;

namespace BussinessLayer.Implement
{
    public class ItemImageService : IItemImageService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly CloudinaryService _cloudinaryService;

        public ItemImageService(IUnitOfWork unitOfWork, CloudinaryService cloudinaryService)
        {
            _unitOfWork = unitOfWork;
            _cloudinaryService = cloudinaryService;
        }

        public ItemImage? GetItemImageById(int id)
        {
            var itemImage = _unitOfWork.ItemImage.Get(i => i.Id == id);
            return itemImage;
        }

        public async Task RemoveItemImage(ItemImage itemImage)
        {
            if (itemImage.ImagePath != null)
            {
                await _cloudinaryService.DeleteImageAsync(itemImage.PublicId);
                _unitOfWork.ItemImage.Remove(itemImage);
                _unitOfWork.Save();
            }
        }
    }
}
