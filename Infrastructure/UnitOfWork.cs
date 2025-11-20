using System;
using ApplicationCore.Domain.Repositories;
using NHibernate;

namespace Infrastructure
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ISession _session;

        public UnitOfWork(ISession session)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
        }

        public void SaveChanges()
        {
            try
            {
                using (var transaction = _session.BeginTransaction())
                {
                    try
                    {
                        _session.Flush();
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error durante la transacción de base de datos", ex);
            }
        }

        public void Dispose()
        {
            _session?.Dispose();
        }
    }
}
