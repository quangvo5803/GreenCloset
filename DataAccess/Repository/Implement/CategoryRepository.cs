using GreenCloset.Data;
using GreenCloset.Models;
using GreenCloset.Repository.Interface;

namespace GreenCloset.Repository.Implement
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        private ApplicationDbContext _db;

        public CategoryRepository(ApplicationDbContext db)
            : base(db)
        {
            _db = db;
        }

        public void Update(Category category)
        {
            _db.Categories.Update(category);
        }
    }
}
