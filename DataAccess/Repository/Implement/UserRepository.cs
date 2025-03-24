using GreenCloset.Data;
using GreenCloset.Models;
using GreenCloset.Repository.Interface;

namespace GreenCloset.Repository.Implement
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        private ApplicationDbContext _db;

        public UserRepository(ApplicationDbContext db)
            : base(db)
        {
            _db = db;
        }

        public void Update(User user)
        {
            _db.Users.Update(user);
        }
    }
}
