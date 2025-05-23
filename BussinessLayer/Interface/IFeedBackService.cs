using DataAccess.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BussinessLayer.Interface
{
    public interface IFeedBackService
    {
        Task SubmitFeedback(int orderId ,int productId, int stars, string? feedbackContent, List<IFormFile>? images, Guid userId);
        public (Product? product, IEnumerable<Feedback> feedbacks) ViewFeedbackProduct(int productId);
    }
}
