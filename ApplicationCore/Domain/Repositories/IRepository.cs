using System.Collections.Generic;

namespace ApplicationCore.Domain.Repositories
{
    public interface IRepository<TEntity, TKey> where TEntity : class
    {
        TEntity? GetById(TKey id);
        IEnumerable<TEntity> GetAll();
        void New(TEntity entity);
        void Modify(TEntity entity);
        void Destroy(TEntity entity);
    }
}
