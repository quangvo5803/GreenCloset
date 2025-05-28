using System.Threading.Tasks;
using BussinessLayer.Interface;
using DataAccess.Models;
using Microsoft.AspNetCore.Http;
using Repository.Implement;
using Utility.Media;

namespace BussinessLayer.Implement
{
    public class FeedBackService : IFeedBackService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly CloudinaryService _cloudinaryService;

        public FeedBackService(IUnitOfWork unitOfWork, CloudinaryService cloudinaryService)
        {
            _unitOfWork = unitOfWork;
            _cloudinaryService = cloudinaryService;
        }

        public async Task SubmitFeedback(
            int orderId,
            int productId,
            int stars,
            string? feedbackContent,
            List<IFormFile>? images,
            Guid userId
        )
        {
            var product = _unitOfWork.Product.Get(p => p.Id == productId);
            if (product == null)
            {
                throw new Exception("Sản phẩm không tồn tại.");
            }

            var submit = new Feedback
            {
                OrderId = orderId,
                ProductId = productId,
                FeedbackStars = stars,
                FeedbackContent = feedbackContent,
                UserId = userId,
            };
            _unitOfWork.Feedback.Add(submit);
            _unitOfWork.Save();

            //Save img
            var uploadFolder = "feedbacks";
            if (images != null || images?.Count > 0)
            {
                foreach (var image in images)
                {
                    var itemImage = new ItemImage
                    {
                        ImagePath = "",
                        ProductId = productId,
                        FeedbackId = submit.Id,
                        PublicId = "",
                    };
                    await _cloudinaryService.UploadImageAsync(image, uploadFolder, itemImage);
                    _unitOfWork.ItemImage.Add(itemImage);
                }
                _unitOfWork.Save();
            }
        }

        public IEnumerable<Feedback> GetAllShopFeedback(Guid shopId)
        {
            var feedbacks = _unitOfWork.Feedback.GetRange(
                f => f.Product != null && f.Product.UserId == shopId,
                includeProperties: "Product"
            );
            return feedbacks;
        }

        public (Product? product, IEnumerable<Feedback> feedbacks) ViewFeedbackProduct(
            int productId
        )
        {
            var product = _unitOfWork.Product.Get(p => p.Id == productId);
            if (product == null)
                return (null, Enumerable.Empty<Feedback>());

            var feedbacks = _unitOfWork.Feedback.GetRange(
                f => f.ProductId == productId,
                includeProperties: "User,Images"
            );

            return (product, feedbacks);
        }
    }
}
