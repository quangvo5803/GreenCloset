using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BussinessLayer.Interface;
using DataAccess.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Repository.Implement;

namespace BussinessLayer.Implement
{
    public class FeedBackService : IFeedBackService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public FeedBackService(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
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
            string uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "image/feedbacks");
            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }
            if (images != null || images?.Count > 0)
            {
                foreach (var steam in images)
                {
                    string uniqueFileName =
                        Guid.NewGuid().ToString() + "_" + Path.GetExtension(steam.FileName);

                    var filePath = Path.Combine(uploadFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await steam.CopyToAsync(fileStream);
                    }

                    var itemImage = new ItemImage
                    {
                        ImagePath = uniqueFileName,
                        ProductId = productId,
                        FeedbackId = submit.Id,
                    };
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
    }
}
