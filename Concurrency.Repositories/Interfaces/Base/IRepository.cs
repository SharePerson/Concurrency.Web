using Concurrency.Entities;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Concurrency.Repositories.Interfaces.Base
{
    public interface IRepository<EntityType>: IAsyncDisposable where EntityType: class
    {
        ConcurrencyDbContext DbContext { get; }
        Task Add(EntityType entity);
        void Update(EntityType entity);
        IQueryable<EntityType> GetMany(Expression<Func<EntityType, bool>> where);
        Task<EntityType> Get(Expression<Func<EntityType, bool>> where);
        void Delete(Expression<Func<EntityType, bool>> where);
        void SetOriginalValue<PropertyType>(EntityType entity, string propertyName, PropertyType value);
    }
}
