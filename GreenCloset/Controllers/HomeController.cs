using BussinessLayer.Interface;
using DataAccess.Models;
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

    public IActionResult About()
    {
        return View();
    }

    public IActionResult ProductDetail(int id, int pageNumber = 1, int pageSize = 5)
    {
        var product = _facadeService.Product.GetProductById(
            id,
            includeProperties: "Categories,ProductAvatar,ProductImages,Feedbacks"
        );

        if (product != null)
        {
            var similarProduct = _facadeService.Product.GetSimilarProduct(
                product,
                includeProperties: "ProductAvatar,Categories"
            );
            ViewBag.SimilarProducts = similarProduct;
            ViewBag.PageNumber = pageNumber;
            ViewBag.PageSize = pageSize;
            return View(product);
        }
        TempData["error"] = "Sản phẩm không tồn tại";
        return RedirectToAction("Index", "Home");
    }

    public IActionResult Shop(int page = 1, int pageSize = 12, ProductFilter? filter = null)
    {
        var productList = _facadeService.Product.GetProductsByFilter(filter);

        //Pagination
        var paginatedProducts = productList.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        var totalProducts = productList.Count();
        ViewBag.Filter = filter;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling((double)totalProducts / pageSize);
        return View(paginatedProducts);
    }
}
