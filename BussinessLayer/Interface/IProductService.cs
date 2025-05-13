using DataAccess.Models;
using Microsoft.AspNetCore.Http;

namespace BussinessLayer.Interface
{
    public interface IProductService
    {
        Product? GetProductById(int id, string? includeProperties = null);
        IEnumerable<Product> GetAllProducts(string? includeProperties = null);
        IEnumerable<Product> GetFeatureProduct(string? includeProperties = null);
        IEnumerable<Product> GetSimilarProduct(Product product, string? includeProperties = null);
        Task AddProduct(
            Guid userId,
            Product product,
            IEnumerable<int>? selectedCategories,
            IFormFile? avatar,
            List<IFormFile>? gallery
        );
        Task UpdateProduct(
            Product product,
            IEnumerable<int>? selectedCategories,
            IFormFile? avatar,
            List<IFormFile>? gallery
        );
        bool DeleteProduct(Product product);
        IEnumerable<Product> GetProductsByFilter(ProductFilter? filter);
    }
}
