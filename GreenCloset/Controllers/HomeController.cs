using BussinessLayer.Interface;
using Microsoft.AspNetCore.Mvc;

namespace GreenCloset.Controllers;

public partial class HomeController : BaseController
{
    public HomeController(IFacedeService facedeService)
        : base(facedeService) { }

    public IActionResult Index()
    {
        var products = _facadeService.Product.GetAllProducts(
            includeProperties: "Categories,ProductAvatar"
        );
        ViewBag.FeatureProducts = _facadeService.Product.GetFeatureProduct(
            includeProperties: "ProductAvatar"
        );
        return View(products);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult ProductDetail(int id, int pageNumber = 1, int pageSize = 5)
    {
        var product = _facadeService.Product.GetProductById(
            id,
            includeProperties: "Categories,ProductAvatar,ProductImages,Feedbacks"
        );
        ViewBag.PageNumber = pageNumber;
        ViewBag.PageSize = pageSize;
        return View(product);
    }
}
