using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace EMT.DAL.Interfaces
{
    public interface IGenericAsyncRepository<T> where T : class
    {
        IDbConnection GetConnection { get; }
        Task<IEnumerable<T>> GetAll();
        Task<T> GetById(object id);
        Task Add(T entity);
        Task DeleteById(object id);
        Task Update(T entity);
    }
}
