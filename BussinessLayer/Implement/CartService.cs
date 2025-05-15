using BussinessLayer.Interface;
using DataAccess.Models;
using Microsoft.AspNetCore.Mvc;
using Repository.Implement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BussinessLayer.Implement
{
    public class CartService : ICartService
    {
        private readonly IUnitOfWork _unitOfWork;
        public CartService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IEnumerable<IGrouping<Guid?, Cart>> GetAllCartGroupedByProductUser(Guid userId)
        {
            var carts = _unitOfWork.Cart.GetRange(
                c => c.UserId == userId,
                includeProperties: "Product,Product.User"
            ).OrderByDescending(c => c.Id).ToList();

            // Group by Product.UserId (assuming Product.UserId is a Guid)
            var groupedCarts = carts.GroupBy(c => c.Product?.UserId);

            return groupedCarts;
        }


        public void DeleteCart(Guid userId, int productId)
        {
            var cartItem = _unitOfWork.Cart.Get(c => c.UserId == userId && c.ProductId == productId);
            if (cartItem != null)
            {
                _unitOfWork.Cart.Remove(cartItem);
                _unitOfWork.Save();
            }
        }

        public void AddToCart(int productId, string? size, DateTime? startDate, DateTime? endDate, int count, string userId)
        {
            var product = _unitOfWork.Product.Get(p => p.Id == productId);
            if (product == null)
            {
                throw new Exception("Sản phẩm không tồn tại.");
            }

            // kiểm tra loại size
            bool isShoe = product.SizeShoe != null && product.SizeShoe.Any();
            bool isClother = product.SizeClother != null && product.SizeClother.Any();

            // xử lý size
            SizeClother? sizeClother = null;
            int? sizeShoe = null;

            if (isShoe)
            {
                sizeShoe = int.TryParse(size, out int parsedSize) ? parsedSize : null;
            }
            else if (isClother)
            {
                sizeClother = Enum.TryParse<SizeClother>(size, out var parsedEnum) ? parsedEnum : null;
            }

            var existCartItem = _unitOfWork.Cart.Get(c =>
                c.UserId.ToString() == userId &&
                c.ProductId == productId &&
                c.SizeClother == sizeClother &&
                c.SizeShoe == sizeShoe &&
                c.StartDate == startDate &&
                c.EndDate == endDate
            );

            if (existCartItem != null)
            {
                existCartItem.Count += count;
                _unitOfWork.Cart.Update(existCartItem);
            }
            else
            {
                var cartItem = new Cart
                {
                    UserId = Guid.Parse(userId),
                    ProductId = productId,
                    Count = count,
                    SizeClother = sizeClother,
                    SizeShoe = sizeShoe,
                    StartDate = startDate,
                    EndDate = endDate,
                };
                _unitOfWork.Cart.Add(cartItem);
            }

            _unitOfWork.Save();
        }

        public void UpdateCart(int productId, int quantity, string? size, string? sizeType, DateTime? startDate, DateTime? endDate, string userId)
        {
            var userGuid = Guid.Parse(userId);
            var product = _unitOfWork.Product.Get(p => p.Id == productId);
            if (product == null)
            {
                throw new Exception("sản phẩm không tồn tại.");
            }

            var cartItem = _unitOfWork.Cart.Get(c =>
                c.ProductId == productId &&
                c.UserId == userGuid
            );

            if (cartItem != null)
            {
                cartItem.Count = quantity;
                if (!string.IsNullOrEmpty(size))
                {
                    if (sizeType == "clother")
                    {
                        if (Enum.TryParse<SizeClother>(size, true, out var clothingSize))
                        {
                            cartItem.SizeClother = clothingSize;
                            cartItem.SizeShoe = null;
                        }
                    }
                    else if (sizeType == "shoe")
                    {
                        if (int.TryParse(size, out var shoeSize))
                        {
                            cartItem.SizeShoe = shoeSize;
                            cartItem.SizeClother = null;
                        }
                    }

                }
                cartItem.StartDate = startDate.HasValue ? startDate : null;
                cartItem.EndDate = endDate.HasValue ? endDate : null;
                _unitOfWork.Cart.Update(cartItem);
                _unitOfWork.Save();  
            }
            else
            {
                throw new Exception("không tìm thấy sản phẩm");
            }
        }

    }
}
