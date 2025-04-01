using DataAccess.Models;

namespace Repository.Interface
{
    public interface IProductRepository : IRepository<Product>
    {
        void Update(Product product);
    }
}
