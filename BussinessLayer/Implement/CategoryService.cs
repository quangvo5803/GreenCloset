using BussinessLayer.Interface;
using DataAccess.Models;
using Repository.Implement;

namespace BussinessLayer.Implement
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Category? GetCategoryById(int id)
        {
            return _unitOfWork.Category.Get(c => c.Id == id, includeProperties: "Products");
        }

        public IEnumerable<Category> GetAllCategories(string? includeProperties = null)
        {
            return _unitOfWork.Category.GetAll(includeProperties);
        }

        public void AddCategory(Category category)
        {
            _unitOfWork.Category.Add(category);
            _unitOfWork.Save();
        }

        public void UpdateCategory(Category category)
        {
            _unitOfWork.Category.Update(category);
            _unitOfWork.Save();
        }

        public void DeleteCategory(Category category)
        {
            _unitOfWork.Category.Remove(category);
            _unitOfWork.Save();
        }
    }
}
