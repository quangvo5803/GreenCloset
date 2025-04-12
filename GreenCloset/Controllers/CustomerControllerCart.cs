using BussinessLayer.Interface;
using Microsoft.AspNetCore.Mvc;

namespace GreenCloset.Controllers
{
    public partial class CustomerController : BaseController
    {
        public CustomerController(IFacedeService facedeService)
        : base(facedeService) { }
        public IActionResult Cart()
        {
            return View();
        }
    }
}
