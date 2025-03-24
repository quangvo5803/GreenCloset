using GreenCloset.Models;

namespace GreenCloset.Repository.Interface
{
    public interface IUserRepository : IRepository<User>
    {
        void Update(User user);
    }
}
