using System;
using System.Collections.Generic;
using System.Linq;
using ApplicationCore.Domain.Repositories;
using NHibernate;

namespace Infrastructure.Repositories
{
    public class GenericRepository<TEntity> : IRepository<TEntity, long> where TEntity : class
    {
        protected readonly ISession _session;

        public GenericRepository(ISession session)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
        }

        public virtual TEntity? GetById(long id)
        {
            if (id <= 0)
                return null;

            return _session.Get<TEntity>(id);
        }

        public virtual IEnumerable<TEntity> GetAll()
        {
            var entityName = typeof(TEntity).Name;
            string hql = $"FROM {entityName}";
            return _session.CreateQuery(hql).List<TEntity>();
        }

        public virtual void New(TEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _session.Save(entity);
        }

        public virtual void Modify(TEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _session.Update(entity);
        }

        public virtual void Destroy(TEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _session.Delete(entity);
        }
    }
}
