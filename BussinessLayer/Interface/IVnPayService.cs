using DataAccess.Models;
using Microsoft.AspNetCore.Http;

namespace BussinessLayer.Interface
{
    public interface IVnPayService
    {
        string CreatePaymentUrl(HttpContext context, VnPaymentRequestModel requestModel);
        VnPaymentResponseModel PaymentExecute(IQueryCollection query);
    }
}
