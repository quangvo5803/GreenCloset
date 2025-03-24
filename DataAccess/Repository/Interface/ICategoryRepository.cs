using GreenCloset.Models;

namespace GreenCloset.Repository.Interface
{
    public interface ICategoryRepository : IRepository<Category>
    {
        void Update(Category category);
    }
}
