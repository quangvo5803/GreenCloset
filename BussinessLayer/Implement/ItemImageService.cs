using BussinessLayer.Interface;
using DataAccess.Models;
using Microsoft.AspNetCore.Hosting;
using Repository.Implement;

namespace BussinessLayer.Implement
{
    public class ItemImageService : IItemImageService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ItemImageService(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

        public ItemImage? GetItemImageById(int id)
        {
            var itemImage = _unitOfWork.ItemImage.Get(i => i.Id == id);
            return itemImage;
        }

        public void RemoveItemImage(ItemImage itemImage)
        {
            string uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "image/products");
            if (itemImage.ImagePath != null)
            {
                var oldAvatarPath = Path.Combine(uploadFolder, itemImage.ImagePath);
                if (File.Exists(oldAvatarPath))
                {
                    File.Delete(oldAvatarPath);
                }
                _unitOfWork.ItemImage.Remove(itemImage);
                _unitOfWork.Save();
            }
        }
    }
}
