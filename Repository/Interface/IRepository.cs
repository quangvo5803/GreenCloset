using System.Linq.Expressions;

namespace Repository.Interface
{
    public interface IRepository<T>
        where T : class
    {
        IEnumerable<T> GetAll(string? includeProperties = null);
        T? Get(Expression<Func<T, bool>> filter);
        public T? Get(Expression<Func<T, bool>> filter, string? includeProperties = null);
        public IEnumerable<T> GetRange(
            Expression<Func<T, bool>> filter,
            string? includeProperties = null
        );
        void Add(T entity);
        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entities);
        void AddRange(IEnumerable<T> entities);
    }
}
