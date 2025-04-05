using BussinessLayer.Interface;
using Microsoft.AspNetCore.Mvc;

namespace GreenCloset.Controllers;

public partial class HomeController : BaseController
{
    public HomeController(IFacedeService facedeService)
        : base(facedeService) { }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }
}
