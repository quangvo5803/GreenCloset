using DataAccess.Models;

namespace Repository.Interface
{
    public interface IFeedbackRepository : IRepository<Feedback>
    {
        void Update(Feedback feedback);
    }
}
