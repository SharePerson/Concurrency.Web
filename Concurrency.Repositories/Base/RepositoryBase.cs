using Concurrency.Entities;
using Concurrency.Repositories.Interfaces.Base;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Concurrency.Repositories.Base
{
    public abstract class RepositoryBase<EntityType> : IRepository<EntityType> where EntityType : class
    {
        private readonly ConcurrencyDbContext dbContext;

        public ConcurrencyDbContext DbContext { get => dbContext; }

        public RepositoryBase(ConcurrencyDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task Add(EntityType entity)
        {
            await dbContext.AddAsync(entity);
        }

        public void Delete(Expression<Func<EntityType, bool>> where)
        {
            dbContext.Remove(where);
        }

        public async Task<EntityType> Get(Expression<Func<EntityType, bool>> where)
        {
            return await dbContext.Set<EntityType>().FirstOrDefaultAsync(where);
        }

        public IQueryable<EntityType> GetMany(Expression<Func<EntityType, bool>> where)
        {
            return dbContext.Set<EntityType>().Where(where);
        }

        public void Update(EntityType entity)
        {
            dbContext.Update(entity);
        }

        public void SetOriginalValue<PropertyType>(EntityType entity, string propertyName, PropertyType value)
        {
            dbContext.Entry(entity).Property(propertyName).OriginalValue = value;
        }

        public async ValueTask DisposeAsync()
        {
            await dbContext.DisposeAsync();
            GC.SuppressFinalize(this);
        }
    }
}
