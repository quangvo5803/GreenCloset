using DataAccess.Models;

namespace Repository.Interface
{
    public interface ICategoryRepository : IRepository<Category>
    {
        void Update(Category category);
    }
}
