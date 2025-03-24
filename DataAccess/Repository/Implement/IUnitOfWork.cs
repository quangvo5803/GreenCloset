using GreenCloset.Repository.Interface;

namespace GreenCloset.Repository.Implement
{
    public interface IUnitOfWork
    {
        ICategoryRepository Category { get; }
        IUserRepository User { get; }
        void Save();
    }
}
