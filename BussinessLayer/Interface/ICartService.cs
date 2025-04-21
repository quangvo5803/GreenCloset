using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BussinessLayer.Interface
{
    public interface ICartService
    {
        IEnumerable<Cart> GetAllCartById(Guid id);
        void DeleteCart(Guid userId, int productId);
        void AddToCart(int productId, string? size, DateTime? startDate, DateTime? endDate, int count, string userId);
        void UpdateCart(int productId, int quantity, string? size, string? sizeType, DateTime? startDate, DateTime? endDate, string userId);
        
    }


}
