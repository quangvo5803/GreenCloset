using BussinessLayer.Interface;
using DataAccess.Models;
using GreenCloset.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Repository.Implement;
using Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BussinessLayer.Implement
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IVnPayService _vpnPayService;
        private readonly EmailSender _emailSender;
        private readonly IConfiguration _configuration;

        public OrderService(IUnitOfWork unitOfWork, IVnPayService vpnPayService, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _vpnPayService = vpnPayService;
            _emailSender = new EmailSender(
            _configuration,
                new LoggerFactory().CreateLogger<EmailSender>()
            );
           
        }

        //view checkout
        public IEnumerable<Cart> GetCartItems(List<int> selectedItems)
        {
            var productsInCart = _unitOfWork.Cart
                .GetRange(c => selectedItems.Contains(c.Id), includeProperties: "Product,Product.ProductAvatar")
                .ToList(); 

            return productsInCart;
        }
        //Payment by COD
        public Order? ProcessOrderByCOD(
            List<int> selectedItems, string phoneNumber, 
            DeliveryOption deliveryOptions, string deliveryAddress,
            PaymentMethod paymentMethod, Guid userId)
        {
            var productsInCart = _unitOfWork.Cart.GetRange
                (c => selectedItems.Contains(c.Id), includeProperties: "Product").ToList();

            if (!productsInCart.Any())
            {
                return null;
            }
            double totalPrice = 0;
            foreach (var item in productsInCart)
            {
                if(item.Product != null)
                {
                    totalPrice += item.Product.Price * item.Count;
                }
            }
            if ( deliveryOptions == DeliveryOption.HomeDelivery)
            {
                totalPrice += 50000;
            }
                var order = CreateOrder(userId, totalPrice, phoneNumber, deliveryOptions, deliveryAddress, paymentMethod, productsInCart);
                _unitOfWork.Order.Add(order);
                var removeCartItem = _unitOfWork.Cart.GetRange(c => selectedItems.Contains(c.Id));
                _unitOfWork.Cart.RemoveRange(removeCartItem);
                _unitOfWork.Save();
            return order;
        }

        //Payment by VnPay
        public string? ProcessOrderByVnPay(
            List<int> selectedItems, string phoneNumber, 
            DeliveryOption deliveryOptions, string deliveryAddress, 
            PaymentMethod paymentMethod, Guid userId, HttpContext httpContext)
        {
            var productsInCart = _unitOfWork.Cart.GetRange
                (c => selectedItems.Contains(c.Id), includeProperties: "Product").ToList();

            if (!productsInCart.Any())
            {
                return null;
            }

            double totalPrice = 0;
            foreach (var item in productsInCart)
            {
                if (item.Product != null)
                {
                    totalPrice += item.Product.Price * item.Count;
                }
            }

            if (deliveryOptions == DeliveryOption.HomeDelivery)
            {
                totalPrice += 50000;
            }

            var order = CreateOrder(userId, totalPrice, phoneNumber, deliveryOptions, deliveryAddress, paymentMethod, productsInCart);
            _unitOfWork.Order.Add(order);
            _unitOfWork.Save();
           
            var vnpayModel = new VnPaymentRequestModel
            {
                OrderId = new Random().Next(10000, 100000),
                Description = "Thanh toán VNPay",
                CreateDate = DateTime.Now,
                Amount = totalPrice,
                Order = order,
            };
            
            return _vpnPayService.CreatePaymentUrl(httpContext, vnpayModel);
        }


        public bool VNPayReturn(IQueryCollection query, string userId, out int orderId)
        {
            orderId = 0;     
            var response = _vpnPayService.PaymentExecute(query);
            if (response == null || response.VnPayResponseCode != "00")
            {
                if (int.TryParse(response?.OrderDescription, out int failOrderId))
                {
                    var orderDetailList = _unitOfWork.OrderDetail.GetRange(o => o.Id == failOrderId);
                    _unitOfWork.OrderDetail.RemoveRange(orderDetailList);
                    _unitOfWork.Save();

                    var order = _unitOfWork.Order.Get(o => o.Id == failOrderId);
                    if(order != null)
                    {
                        _unitOfWork.Order.Remove(order);
                        _unitOfWork.Save();
                    }
                }
                return false;
            }

            if (int.TryParse(response.OrderDescription, out int orderSuccessId))
            {
                var purchasedProductIds = _unitOfWork.OrderDetail
                    .GetRange(od =>  od.OrderId == orderSuccessId, includeProperties: "Product")
                    .Select(od => od.ProductId)
                    .ToList();
  
                var cartItemRemove = _unitOfWork.Cart
                    .GetRange(c => c.UserId.ToString() == userId && purchasedProductIds.Contains(c.ProductId));

                _unitOfWork.Cart.RemoveRange(cartItemRemove);
                _unitOfWork.Save();

                orderId = orderSuccessId;
                return true;
                
            }

            return false;
        }
        private Order CreateOrder(
            Guid userId,
            double totalPrice,
            string phoneNumber,
            DeliveryOption deliveryOptions,
            string deliveryAddress,
            PaymentMethod paymentMethod,
            IEnumerable<Cart> productsInCart)
        {
            return new Order
            {
                UserId = userId,
                OrderDate = DateTime.Now,
                TotalPrice = totalPrice,
                PhoneNumber = phoneNumber,
                DeliveryOption = deliveryOptions,
                ShippingAddress = deliveryAddress,
                PaymentMethod = paymentMethod,
                OrderDetails = productsInCart.Select(p => new OrderDetail
                {
                    ProductId = p.ProductId,
                    Quantity = p.Count,
                    UnitPrice = p.Product != null ? p.Product.Price : 0,
                }).ToList()
            };
        }


    }
}
