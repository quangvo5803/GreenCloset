using DataAccess.Models;

namespace Repository.Interface
{
    public interface IUserRepository : IRepository<User>
    {
        void Update(User user);
    }
}
