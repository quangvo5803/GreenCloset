using Repository.Interface;

namespace Repository.Implement
{
    public interface IUnitOfWork
    {
        ICategoryRepository Category { get; }
        IUserRepository User { get; }
        IProductRepository Product { get; }
        IOrderRepository Order { get; }
        IOrderDetailRepository OrderDetail { get; }
        ICartRepository Cart { get; }
        IItemImageRepository ItemImage { get; }
        void Save();
    }
}
