using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;
using NHibernate;

namespace Infrastructure.Repositories
{
    public class UbicacionRepository : GenericRepository<Ubicacion>, IUbicacionRepository
    {
        public UbicacionRepository(ISession session) : base(session)
        {
        }
    }
}
