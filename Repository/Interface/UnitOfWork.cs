using DataAccess.Data;
using Repository.Implement;

namespace Repository.Interface
{
    public class UnitOfWork : IUnitOfWork
    {
        public IUserRepository User { get; private set; }
        public ICategoryRepository Category { get; private set; }
        public IProductRepository Product { get; private set; }
        public IItemImageRepository ItemImage { get; private set; }
        public IOrderRepository Order { get; private set; }
        public IOrderDetailRepository OrderDetail { get; private set; }
        public ICartRepository Cart { get; private set; }
        public IFeedbackRepository Feedback { get; private set; }
        private ApplicationDbContext _db;

        public UnitOfWork(ApplicationDbContext db)
        {
            _db = db;
            User = new UserRepository(_db);
            Category = new CategoryRepository(_db);
            Product = new ProductRepository(_db);
            ItemImage = new ItemImageRepository(_db);
            Order = new OrderRepository(_db);
            OrderDetail = new OrderDetailRepository(_db);
            Cart = new CartRepository(_db);
            Feedback = new FeedbackRepository(_db);
        }

        public void Save()
        {
            _db.SaveChanges();
        }
    }
}
