using BussinessLayer.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GreenCloset.Controllers
{
    public class BaseController : Controller
    {
        public readonly IFacedeService _facadeService;

        public BaseController(IFacedeService facadeService)
        {
            _facadeService = facadeService;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            ViewBag.Categories = _facadeService.Category.GetAllCategories();
            base.OnActionExecuting(context);
        }
    }
}
