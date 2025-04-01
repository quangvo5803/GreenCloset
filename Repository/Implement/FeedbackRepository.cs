using DataAccess.Data;
using DataAccess.Models;
using Repository.Interface;

namespace Repository.Implement
{
    public class FeedbackRepository : Repository<Feedback>, IFeedbackRepository
    {
        private readonly ApplicationDbContext _db;

        public FeedbackRepository(ApplicationDbContext db)
            : base(db)
        {
            _db = db;
        }

        public void Update(Feedback feedback)
        {
            _db.Feedbacks.Update(feedback);
        }
    }
}
