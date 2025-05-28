using Microsoft.AspNetCore.Mvc;

namespace GreenCloset.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error/{statusCode}")]
        public IActionResult HttpStatusCodeHandler(int statusCode)
        {
            switch (statusCode)
            {
                case 404:
                    return View("NotFound");
                default:
                    return View("Error"); // View cho các lỗi khác
            }
        }

        [Route("Error")]
        public IActionResult Error()
        {
            return View(); // View cho lỗi 500 hoặc exception chung
        }
    }
}
