using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess.Models;
using Microsoft.AspNetCore.Http;

namespace BussinessLayer.Interface
{
    public interface IFeedBackService
    {
        Task SubmitFeedback(
            int orderId,
            int productId,
            int stars,
            string? feedbackContent,
            List<IFormFile>? images,
            Guid userId
        );
        IEnumerable<Feedback> GetAllShopFeedback(Guid shopId);
    }
}
