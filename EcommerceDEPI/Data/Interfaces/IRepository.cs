using EcommerceDEPI.Data;
using EcommerceDEPI.Models;
using System.Linq.Expressions;

namespace EcommerceDEPI.Data.Interfaces
{
    public interface IRepository<T> where T : class
    {
        IQueryable<T> GetAll();
        Task<T> GetByIdAsync(params object[] keyValues);
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(params object[] keyValues);
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    }
}