using DataAccess.Models;
using Microsoft.AspNetCore.Http;

namespace BussinessLayer.Interface
{
    public interface IProductService
    {
        Product? GetProductById(int id, string? includeProperties = null);
        IEnumerable<Product> GetAllProducts(string? includeProperties = null);
        IEnumerable<Product> GetFeatureProduct(string? includeProperties = null);
        IEnumerable<Product> GetProductsByCategoryId(
            int categoryId,
            string? includeProperties = null
        );
        Task AddProduct(
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
    }
}
