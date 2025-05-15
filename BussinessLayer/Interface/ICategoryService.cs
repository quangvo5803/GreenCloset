using DataAccess.Models;

namespace BussinessLayer.Interface
{
    public interface ICategoryService
    {
        Category? GetCategoryById(int id);
        IEnumerable<Category> GetAllCategories(string? includeProperties = null);
        void AddCategory(Category category);
        void UpdateCategory(Category category);
        void DeleteCategory(Category category);
    }
}
