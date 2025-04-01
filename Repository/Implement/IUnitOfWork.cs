using Repository.Interface;

namespace Repository.Implement
{
    public interface IUnitOfWork
    {
        ICategoryRepository Category { get; }
        IUserRepository User { get; }
        void Save();
    }
}
