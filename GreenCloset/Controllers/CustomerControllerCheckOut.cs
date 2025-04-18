using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenCloset.Controllers
{
    [Authorize(Roles = "Customer")]
    public partial class CustomerController : BaseController
    {
        public IActionResult Checkout()
        {
            return View();
        }
    }
}
