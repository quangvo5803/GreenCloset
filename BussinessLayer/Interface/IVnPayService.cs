using DataAccess.Models;
using Microsoft.AspNetCore.Http;

namespace BussinessLayer.Interface
{
    public interface IVnPayService
    {
        string CreatePaymentUrl(
            HttpContext context,
            VnPaymentRequestModel requestModel,
            string type
        );
        VnPaymentResponseModel PaymentExecute(IQueryCollection query);
        string GenerateQrCodeBase64(string paymentUrl);
    }
}
