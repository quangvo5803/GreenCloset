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
        ViewBag.PageNumber = pageNumber;
        ViewBag.PageSize = pageSize;
        return View(product);
    }

    public IActionResult Shop(
        int page = 1,
        int pageSize = 12,
        List<int>? categoryIds = null,
        List<ProductColor>? colors = null,
        List<SizeClother>? clotherSizes = null,
        List<int>? shoeSizes = null,
        double? priceFrom = null,
        double? priceTo = null
    )
    {
        var productList = _facadeService.Product.GetAllProducts(
            includeProperties: "Categories,ProductAvatar"
        );
        if (categoryIds != null && categoryIds.Any())
        {
            productList = productList.Where(p =>
                p.Categories != null && p.Categories.Any(c => categoryIds.Contains(c.Id))
            );
        }
        if (colors != null && colors.Any())
        {
            productList = productList.Where(p =>
                p.Color.HasValue && colors.Contains(p.Color.Value)
            );
        }
        if (clotherSizes != null && clotherSizes.Any())
        {
            productList = productList.Where(p =>
                p.SizeClother != null && p.SizeClother.Any(s => clotherSizes.Contains(s))
            );
        }
        if (shoeSizes != null && shoeSizes.Any())
        {
            productList = productList.Where(p =>
                p.SizeShoe != null && p.SizeShoe.Any(s => shoeSizes.Contains(s))
            );
        }
        if (priceFrom.HasValue)
        {
            productList = productList.Where(p => p.Price >= priceFrom);
        }
        if (priceTo.HasValue)
        {
            productList = productList.Where(p => p.Price <= priceTo);
        }

        //Pagination
        var paginatedProducts = productList.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        var totalProducts = productList.Count();
        ViewBag.SelectedCategoryIds = categoryIds ?? new List<int>();
        ViewBag.SelectedColors = colors ?? new List<ProductColor>();
        ViewBag.SelectedClotherSizes = clotherSizes ?? new List<SizeClother>();
        ViewBag.SelectedShoeSizes = shoeSizes ?? new List<int>();
        ViewBag.PriceFrom = priceFrom;
        ViewBag.PriceTo = priceTo;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling((double)totalProducts / pageSize);
        return View(paginatedProducts);
    }
}
